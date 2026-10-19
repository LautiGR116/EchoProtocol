"""Disposable Blender FBX roundtrip. Writes only this folder's validation.json."""
import bpy,json,math,hashlib
from pathlib import Path
from mathutils import Vector
from mathutils.bvhtree import BVHTree
BASE=Path(__file__).resolve().parent
manifest=json.loads((BASE/'manifest.json').read_text());results={};errors=[]
def finite(v):return all(math.isfinite(float(x)) for x in v)
def check(ok,message):
 if not ok:errors.append(message)
for name,a in manifest['assets'].items():
 for o in list(bpy.data.objects):bpy.data.objects.remove(o,do_unlink=True)
 for data in (bpy.data.meshes,bpy.data.materials):
  for d in list(data):
   if d.users==0:data.remove(d)
 path=BASE/a['file'];bpy.ops.import_scene.fbx(filepath=str(path));objects=list(bpy.context.scene.objects);meshes=[o for o in objects if o.type=='MESH'];bpy.context.view_layer.update()
 check(len(meshes)==1,name+': mesh count');verts=[];faces=[];tris=0;uv=True;mat=True
 for o in meshes:
  o.data.calc_loop_triangles();offset=len(verts);verts += [o.matrix_world@v.co for v in o.data.vertices];faces += [tuple(offset+i for i in t.vertices) for t in o.data.loop_triangles];tris+=len(o.data.loop_triangles)
  uv &= bool(o.data.uv_layers) and all(finite(v.uv) for l in o.data.uv_layers for v in l.data)
  mat &= all(m and finite(m.diffuse_color) and math.isfinite(m.roughness) and math.isfinite(m.metallic) for m in o.data.materials) and all(0<=p.material_index<len(o.data.materials) for p in o.data.polygons)
 check(all(finite(p) for p in verts),name+': finite vertices');check(uv,name+': finite UVs');check(mat,name+': materials')
 bounds=[[min(p[i] for p in verts),max(p[i] for p in verts)] for i in range(3)];err=max(abs(bounds[i][j]-a['bounds_blender_xyz'][i][j]) for i in range(3) for j in range(2));check(err<.0001,name+': bounds')
 check(tris==a['mesh_objects'][0]['triangles'],name+': triangles');check({m.name for o in meshes for m in o.data.materials}==set(a['materials']),name+': material names')
 anchor_error=0
 for key,p in a['anchors_blender_xyz'].items():
  o=next((o for o in objects if o.name==key),None);check(o is not None,name+': missing '+key)
  if o:anchor_error=max(anchor_error,max(abs(o.matrix_world.translation[i]-p[i]) for i in range(3)))
 check(anchor_error<.0001,name+': anchors')
 tree=BVHTree.FromPolygons(verts,faces,all_triangles=True);rays={}
 def ray(label,origin,direction,length,should_hit=False):
  p,n,i,d=tree.ray_cast(Vector(origin),Vector(direction),length);hit=i is not None;rays[label]={'hit':hit,'distance':d};check(hit==should_hit,name+': '+label)
 if name=='Downpipe27':
  ray('upper bore genuinely open',(0,-.11,2.76),(0,0,-1),.25)
  ray('outlet genuinely open',(0,-.36,.07),(0,1,0),.11)
 if name=='GutterStraight1m':ray('trough has floor',(0,-.11,.20),(0,0,-1),.25,True)
 if name=='GutterOutlet1m':ray('outlet passes through floor',(0,-.11,.20),(0,0,-1),.30)
 if name=='Drain1m':
  ray('grate slot open',(0,0,.05),(0,0,-1),.07)
  ray('actual channel below slot',(0,0,.05),(0,0,-1),.10,True)
 if name=='MineralTile0744':
  check(abs(bounds[0][1]-bounds[0][0]-.744)<.0001 and abs(bounds[1][1]-bounds[1][0]-.744)<.0001 and abs(bounds[2][1]-bounds[2][0]-.04)<.0001,name+': requested dimensions')
 results[name]={'triangles':tris,'bounds_blender_xyz':bounds,'bounds_error':err,'anchor_error':anchor_error,'finite_uvs':uv,'finite_materials':mat,'rays':rays,'sha256':hashlib.sha256(path.read_bytes()).hexdigest()}
check(sum(a['triangles'] for a in results.values())<=10000,'kit triangle budget10000')
report={'pass':not errors,'errors':errors,'scope':'Blender reimport numeric geometry/UV/material/anchors and short physical bore/slot rays. No Unity or human visual approval.','assets':results,'total_triangles':sum(a['triangles'] for a in results.values())}
(BASE/'validation.json').write_text(json.dumps(report,indent=2)+'\n');print('RAINWATER VALIDATION',json.dumps({'pass':not errors,'errors':errors,'triangles':report['total_triangles']}),flush=True)
if errors:raise RuntimeError('; '.join(errors))
