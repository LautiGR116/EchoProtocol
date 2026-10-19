"""FBX round-trip evidence only; does not control Unity or claim a playtest."""
import bpy,os,json,hashlib,math
from mathutils import Vector
BASE=os.path.dirname(os.path.abspath(__file__))
m=json.load(open(os.path.join(BASE,'manifest.json')))
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
p=os.path.join(BASE,m['model']);bpy.ops.import_scene.fbx(filepath=p)
meshes=[o for o in bpy.context.scene.objects if o.type=='MESH'];vertices=[o.matrix_world@v.co for o in meshes for v in o.data.vertices]
bounds=[[min(v[k] for v in vertices),max(v[k] for v in vertices)] for k in range(3)]
error=max(abs(x-y) for a,b in zip(bounds,m['bounds_source_xyz']) for x,y in zip(a,b))
materials={}
for o in meshes:
 for poly in o.data.polygons:
  mat=o.data.materials[poly.material_index].name;materials[mat]=materials.get(mat,0)+1
checks={'scope':'Reimport in Blender 5.2 only; no Unity orientation, shader, movement or comfort claim','bytes':os.path.getsize(p),'sha256':hashlib.sha256(open(p,'rb').read()).hexdigest(),'mesh_count':len(meshes),'armature_count':len([o for o in bpy.context.scene.objects if o.type=='ARMATURE']),'finite_vertices':all(math.isfinite(x) for v in vertices for x in v),'max_bounds_error_m':error,'height_m':bounds[2][1]-bounds[2][0],'material_polygon_counts':materials,'pass':len(meshes)==1 and error<.0001 and all(math.isfinite(x) for v in vertices for x in v) and 1.74<bounds[2][1]-bounds[2][0]<1.78 and all(any(n.startswith(wanted) for n in materials) for wanted in ['FIG_Jacket','FIG_Trousers','FIG_Skin','FIG_Hair','FIG_Shoe'])}
json.dump(checks,open(os.path.join(BASE,'roundtrip-checks.json'),'w'),indent=2);print(json.dumps(checks,indent=2))
if not checks['pass']:raise RuntimeError('Figure FBX check failed')
