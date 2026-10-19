"""FBX metric, material, UV, hierarchy and existing face-clearance checks."""
import bpy,os,json,math,hashlib
from mathutils import Vector
BASE=os.path.dirname(os.path.abspath(__file__));man=json.load(open(os.path.join(BASE,'manifest.json')));results=[]
for name,entry in man['assets'].items():
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 for mesh in list(bpy.data.meshes):
  if mesh.users==0:bpy.data.meshes.remove(mesh)
 for mat in list(bpy.data.materials):
  if mat.users==0:bpy.data.materials.remove(mat)
 path=os.path.join(BASE,entry['file']);bpy.ops.import_scene.fbx(filepath=path)
 objects=list(bpy.context.scene.objects);meshes=[o for o in objects if o.type=='MESH'];coords=[o.matrix_world@v.co for o in meshes for v in o.data.vertices]
 bounds=[[min(p[i] for p in coords),max(p[i] for p in coords)] for i in range(3)];expected=entry['bounds_blender_xyz'];bounds_error=max(abs(bounds[i][j]-expected[i][j]) for i in range(3) for j in range(2))
 finite=all(math.isfinite(x) for p in coords for x in p)
 uv=all(o.data.uv_layers and all(math.isfinite(x) for loop in o.data.uv_layers.active.data for x in loop.uv) for o in meshes)
 materials=sorted({m.name for o in meshes for m in o.data.materials if m});missing_materials=sorted(set(entry['materials'])-set(materials))
 anchor_errors={}
 for key,p in entry['anchors_blender_xyz'].items():
  obj=next((o for o in objects if o.name==key),None);anchor_errors[key]=max(abs(x-y) for x,y in zip(obj.matrix_world.translation,p)) if obj else 1000
 pivots={}
 for data in entry['meshes']:
  obj=next((o for o in meshes if o.name==data['name']),None);pivots[data['name']]=max(abs(x-y) for x,y in zip(obj.matrix_world.translation,data['pivot'])) if obj else 1000
 # All new geometry is behind every existing title/instruction/pilot back plane.
 # Source fronts are negative Y; therefore minimum mesh Y must remain greater.
 min_y=min(p.y for p in coords)
 front_clearance=.0
 if name=='WallReviewHousing':front_clearance=min_y-(-.061)
 else:front_clearance=min_y-(-.065) # Existing pilot backmost plane, not its luminous front.
 clean=finite and uv and not missing_materials and bounds_error<.0001 and max(anchor_errors.values())<.0001 and max(pivots.values())<.0001 and front_clearance>.003
 results.append({'asset':name,'bytes':os.path.getsize(path),'sha256':hashlib.sha256(open(path,'rb').read()).hexdigest(),'mesh_count':len(meshes),'triangles':sum(len(f.vertices)-2 for o in meshes for f in o.data.polygons),'finite_vertices':finite,'finite_uvs':bool(uv),'missing_materials':missing_materials,'bounds_error_m':bounds_error,'mesh_pivot_errors_m':pivots,'anchor_errors_m':anchor_errors,'existing_text_or_pilot_back_plane_clearance_m':front_clearance,'pass':bool(clean)})
 print(name,'PASS' if clean else 'FAIL',flush=True)
result={'scope':'FBX reimport in Blender, metric/UV/material/anchor/front clearance evidence. Does not establish Unity import mirror correction, material render, readability, interaction, desk contact or human approval.','all_pass':all(r['pass'] for r in results),'results':results}
json.dump(result,open(os.path.join(BASE,'roundtrip-checks.json'),'w'),indent=2)
print('ALL PASS',result['all_pass'],flush=True)
if not result['all_pass']:raise RuntimeError('Threshold instrument FBX verification failed')
