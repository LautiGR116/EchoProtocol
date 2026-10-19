"""Reimport each FBX into Blender and check metric dimensions, materials, mesh and pivots.
This is an FBX round-trip check, not Unity runtime or player evidence.
"""
import bpy,os,json,math,hashlib
from mathutils import Vector
BASE=os.path.dirname(os.path.abspath(__file__))
man=json.load(open(os.path.join(BASE,'manifest.json')));results=[]
for name,asset in man['assets'].items():
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 path=os.path.join(BASE,asset['file']);bpy.ops.import_scene.fbx(filepath=path)
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
 vertices=[o.matrix_world@v.co for o in meshes for v in o.data.vertices]
 bounds=[[min(p[i] for p in vertices),max(p[i] for p in vertices)] for i in range(3)]
 dims=[b-a for a,b in bounds];expected=asset['dimensions_width_depth_height']
 dimension_error=max(abs(a-b) for a,b in zip(dims,expected))
 finite=all(math.isfinite(x) for o in meshes for v in o.data.vertices for x in v.co)
 missing_materials=[o.name for o in meshes if not o.data.materials or any(m is None for m in o.data.materials)]
 pivots=[]
 for source in asset['mesh_objects']:
  target=next((o for o in meshes if o.name==source['name']),None)
  error=max(abs(a-b) for a,b in zip(target.matrix_world.translation,source['pivot_blender_xyz'])) if target else 1000
  pivots.append({'name':source['name'],'max_position_error_m':error})
 results.append({'asset':name,'file_sha256':hashlib.sha256(open(path,'rb').read()).hexdigest(),'bytes':os.path.getsize(path),'dimension_error_m':dimension_error,'bounds_xyz':bounds,'finite_vertices':finite,'missing_materials':missing_materials,'pivots':pivots,'pass':finite and not missing_materials and dimension_error<.0001 and all(p['max_position_error_m']<.0001 for p in pivots)})
print(json.dumps(results,indent=2))
json.dump({'scope':'FBX reimport in Blender 5.2; does not claim Unity/playtest verification','all_pass':all(x['pass'] for x in results),'results':results},open(os.path.join(BASE,'roundtrip-checks.json'),'w'),indent=2)
if not all(x['pass'] for x in results):raise RuntimeError('Roundtrip validation failed')
