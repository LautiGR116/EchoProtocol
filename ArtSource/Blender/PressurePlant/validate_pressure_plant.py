"""FBX roundtrip, metric envelope, UV and material checks. Not Unity/gameplay QA."""
import bpy,os,json,math,hashlib
from mathutils import Vector
BASE=os.path.dirname(os.path.abspath(__file__))
manifest=json.load(open(os.path.join(BASE,'manifest.json')));results=[]
for name,a in manifest['assets'].items():
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 for old_mesh in list(bpy.data.meshes):
  if old_mesh.users==0:bpy.data.meshes.remove(old_mesh)
 for material in list(bpy.data.materials):
  if material.users==0:bpy.data.materials.remove(material)
 path=os.path.join(BASE,a['file']);bpy.ops.import_scene.fbx(filepath=path)
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH'];coords=[o.matrix_world@v.co for o in meshes for v in o.data.vertices]
 bounds=[[min(v[i] for v in coords),max(v[i] for v in coords)] for i in range(3)];dimensions=[b-a for a,b in bounds]
 errors=[abs(x-y) for x,y in zip(dimensions,a['dimensions_width_depth_height'])]
 finite=all(math.isfinite(x) for v in coords for x in v)
 materials=sorted({m.name for o in meshes for m in o.data.materials if m});missing=set(a['materials'])-set(materials)
 uv=all(o.data.uv_layers and all(math.isfinite(x) for loop in o.data.uv_layers.active.data for x in loop.uv) for o in meshes)
 nonzero_uv=all(any(abs(loop.uv.x)>.001 or abs(loop.uv.y)>.001 for loop in o.data.uv_layers.active.data) for o in meshes if o.data.uv_layers)
 root_errors=[]
 for source in a['mesh_objects']:
  o=next((obj for obj in meshes if obj.name==source['name']),None)
  root_errors.append(max(abs(x-y) for x,y in zip(o.matrix_world.translation,source['pivot_blender_xyz'])) if o else 1000)
 extra={}
 if name=='ServiceCabinet':
  extra['exact_gameplay_envelope_error_m']=max(abs(x-y) for x,y in zip(dimensions,[1.7,6,2.7]))
  misses=0
  for y in [-2.8,0,2.8]:
   for z in [.25,1.65,2.60]:
    origin=Vector((-1.1,y,z));direction=Vector((1,0,0));blocked=False
    for o in meshes:
     inv=o.matrix_world.inverted();hit,point,normal,index=o.ray_cast(inv@origin,inv.to_3x3()@direction,distance=2.2)
     blocked=blocked or hit
    misses+=0 if blocked else 1
  extra['lateral_occlusion_misses_9_rays']=misses
 if name=='Receiver':extra['height_error_m']=abs(dimensions[2]-2.35)
 ok=finite and uv and nonzero_uv and not missing and max(errors)<.0001 and max(root_errors)<.0001 and all(v<.0001 for v in extra.values())
 r={'asset':name,'file_sha256':hashlib.sha256(open(path,'rb').read()).hexdigest(),'bytes':os.path.getsize(path),'mesh_count':len(meshes),'triangles':sum(len(f.vertices)-2 for o in meshes for f in o.data.polygons),'bounds_blender_xyz':bounds,'dimensions_width_depth_height':dimensions,'bounds_roundtrip_error_m':max(errors),'root_error_m':max(root_errors),'finite_vertices':finite,'finite_populated_uvs':bool(uv and nonzero_uv),'missing_materials':list(missing),'checks':extra,'pass':bool(ok)}
 results.append(r);print(name,'PASS' if ok else 'FAIL',flush=True)
result={'scope':'Reimport in Blender 5.2; checks metric data, UVs, material slots, floor roots. Does not establish Unity facing, collision, light, oclusion, performance, playtest or engineering certification.','all_pass':all(r['pass'] for r in results),'results':results}
json.dump(result,open(os.path.join(BASE,'roundtrip-checks.json'),'w'),indent=2)
print('ALL PASS',result['all_pass'],flush=True)
if not result['all_pass']:raise RuntimeError('Pressure kit validation failed')
