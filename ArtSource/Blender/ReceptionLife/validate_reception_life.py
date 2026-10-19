"""Metric FBX roundtrip, UV/material/root checks, and explicit 03:17 hand geometry."""
import bpy,os,json,math,hashlib
from mathutils import Vector
BASE=os.path.dirname(os.path.abspath(__file__));man=json.load(open(os.path.join(BASE,'manifest.json')));results=[]
for name,a in man['assets'].items():
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 for old in list(bpy.data.meshes):
  if old.users==0:bpy.data.meshes.remove(old)
 for old in list(bpy.data.materials):
  if old.users==0:bpy.data.materials.remove(old)
 path=os.path.join(BASE,a['file']);bpy.ops.import_scene.fbx(filepath=path)
 objects=list(bpy.context.scene.objects);meshes=[o for o in objects if o.type=='MESH'];coords=[o.matrix_world@v.co for o in meshes for v in o.data.vertices]
 bounds=[[min(p[i] for p in coords),max(p[i] for p in coords)] for i in range(3)];bound_error=max(abs(bounds[i][j]-a['bounds_blender_xyz'][i][j]) for i in range(3) for j in range(2))
 finite=all(math.isfinite(x) for p in coords for x in p);uvs=all(o.data.uv_layers and all(math.isfinite(x) for l in o.data.uv_layers.active.data for x in l.uv) for o in meshes)
 materials={m.name for o in meshes for m in o.data.materials if m};missing=sorted(set(a['materials'])-materials);anchors={}
 for key,p in a['anchors_blender_xyz'].items():
  o=next((o for o in objects if o.name==key),None);anchors[key]=max(abs(x-y) for x,y in zip(o.matrix_world.translation,p)) if o else 1000
 checks={}
 if name!='WallClock0317':checks['floor_error_m']=abs(bounds[2][0])
 else:
  for suffix,angle in [('HOUR_TIP',98.5),('MINUTE_TIP',102.0)]:
   marker=next(o for o in objects if o.name==name+'__'+suffix);point=marker.matrix_world.translation;actual=math.degrees(math.atan2(point.x,point.z))%360;checks[suffix+'_angle_error_deg']=abs(actual-angle);checks[suffix+'_nearest_geometry_m']=min((p-point).length for p in coords)
 pass_extra=all(v<.0001 for k,v in checks.items() if 'angle' not in k and 'geometry' not in k) and all(v<.001 for k,v in checks.items() if 'angle' in k) and all(v<.002 for k,v in checks.items() if 'geometry' in k)
 ok=finite and uvs and not missing and bound_error<.0001 and max(anchors.values())<.0001 and len(meshes)==len(a['mesh_objects']) and pass_extra
 results.append({'asset':name,'bytes':os.path.getsize(path),'sha256':hashlib.sha256(open(path,'rb').read()).hexdigest(),'mesh_count':len(meshes),'triangles':sum(len(f.vertices)-2 for o in meshes for f in o.data.polygons),'finite_vertices':finite,'finite_uvs':bool(uvs),'missing_materials':missing,'bounds_error_m':bound_error,'max_anchor_error_m':max(anchors.values()),'checks':checks,'pass':bool(ok)})
 print(name,'PASS' if ok else 'FAIL',flush=True)
result={'scope':'Blender FBX reimport; metric data, UV/material slots, separated coat and rack, source roots, clock hands and anchors. Not Unity integration, game-view readability, collision or human approval.','all_pass':all(r['pass'] for r in results),'results':results}
json.dump(result,open(os.path.join(BASE,'roundtrip-checks.json'),'w'),indent=2);print('ALL PASS',result['all_pass'],flush=True)
if not result['all_pass']:raise RuntimeError('Reception kit validation failed')
