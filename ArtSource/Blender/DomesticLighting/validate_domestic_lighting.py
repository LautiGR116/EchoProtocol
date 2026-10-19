"""Read exported FBX back into Blender and verify source/assembly contract."""
import bpy, os, json, math, hashlib
from mathutils import Vector
BASE=os.path.dirname(os.path.abspath(__file__));m=json.load(open(os.path.join(BASE,'manifest.json')));a=m['assets']['FloorLamp'];path=os.path.join(BASE,a['file'])
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);bpy.ops.import_scene.fbx(filepath=path)
objects=list(bpy.context.scene.objects);meshes=[o for o in objects if o.type=='MESH'];coords=[o.matrix_world@v.co for o in meshes for v in o.data.vertices]
bounds=[[min(p[i] for p in coords),max(p[i] for p in coords)] for i in range(3)];error=max(abs(bounds[i][j]-a['bounds_blender_xyz'][i][j]) for i in range(3) for j in range(2))
anchors={}
for name,p in a['anchors_blender_xyz'].items():
 o=next((o for o in objects if o.name==name),None);anchors[name]=max(abs(x-y) for x,y in zip(o.matrix_world.translation,p)) if o else 999
finite=all(math.isfinite(x) for p in coords for x in p)
uvs=all(o.data.uv_layers and all(math.isfinite(x) for l in o.data.uv_layers.active.data for x in l.uv) for o in meshes)
materials={v.name for o in meshes for v in o.data.materials if v};missing=sorted(set(a['materials'])-materials)
named={o.name:o for o in meshes};needed=[x['name'] for x in a['mesh_objects']];missing_meshes=sorted(set(needed)-set(named))
inner=named.get('ShadeInner');outer=named.get('ShadeOuter')
# The liner must face into the hollow shade and the exterior away from it.
normal_checks={}
for o,label,sign in [(inner,'inner',-1),(outer,'outer',1)]:
 vals=[]
 for p in o.data.polygons:
  mat=o.data.materials[p.material_index].name
  if mat not in ['DL_InnerDiffuser','DL_ShadeLinen']:continue
  centre=sum((o.data.vertices[i].co for i in p.vertices),Vector())/len(p.vertices)
  radial=Vector((centre.x,centre.y,0)).normalized();vals.append(p.normal.dot(radial)*sign)
 normal_checks[label+'_min_radial_alignment']=min(vals) if vals else -1
source=next(o for o in objects if o.name=='FloorLamp__BULB_SOURCE').matrix_world.translation
bulb=named['OpalBulb'];bulb_points=[bulb.matrix_world@v.co for v in bulb.data.vertices]
source_inside_bulb=all(min(p[i] for p in bulb_points)<source[i]<max(p[i] for p in bulb_points) for i in range(3))
cord=named['CordToRearWall'];cord_min=min((cord.matrix_world@v.co).z for v in cord.data.vertices)
checks={'floor_error_m':abs(bounds[2][0]),'cord_floor_error_m':abs(cord_min),'light_anchor_inside_bulb_bounds':source_inside_bulb,**normal_checks}
ok=finite and bool(uvs) and not missing and not missing_meshes and error<.0001 and max(anchors.values())<.0001 and checks['floor_error_m']<.0001 and checks['cord_floor_error_m']<.0002 and source_inside_bulb and all(v>.7 for v in normal_checks.values())
r={'scope':'Blender FBX reimport, named separate surfaces, UVs/materials, dimensions, source/floor/wall anchors and inward liner normals. Not Unity shading, emission/circuit wiring, collision, Game-view or human approval.','all_pass':bool(ok),'bytes':os.path.getsize(path),'sha256':hashlib.sha256(open(path,'rb').read()).hexdigest(),'triangles':sum(len(p.vertices)-2 for o in meshes for p in o.data.polygons),'mesh_count':len(meshes),'missing_meshes':missing_meshes,'finite_vertices':finite,'finite_uvs':bool(uvs),'missing_materials':missing,'bounds_error_m':error,'max_anchor_error_m':max(anchors.values()),'checks':checks}
json.dump(r,open(os.path.join(BASE,'roundtrip-checks.json'),'w'),indent=2);print(json.dumps(r,indent=2),flush=True)
if not ok:raise RuntimeError('Floor lamp validation failed')
