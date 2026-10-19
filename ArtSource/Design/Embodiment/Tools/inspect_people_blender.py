import bpy,json,math
from pathlib import Path
from mathutils import Vector
folder=Path('/Users/lautigr116/EchoProtocol-licensed/Renderpeople-20261009')
reports=[]
for name,path in [('Eric',folder/'Eric/rp_eric_rigged_001_u3d.fbx'),('Nathan',folder/'Nathan/rp_nathan_animated_003_walking.fbx')]:
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 bpy.ops.import_scene.fbx(filepath=str(path),use_anim=True)
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH'];arms=[o for o in bpy.context.scene.objects if o.type=='ARMATURE']
 points=[o.matrix_world@Vector(x) for o in meshes for x in o.bound_box]
 rec={'name':name,'meshes':[{'name':o.name,'vertices':len(o.data.vertices),'triangles':sum(len(p.vertices)-2 for p in o.data.polygons),'uvLayers':len(o.data.uv_layers),'materials':[m.name for m in o.data.materials],'dimensions':list(o.dimensions),'scale':list(o.scale),'parent':o.parent.name if o.parent else None}for o in meshes],
 'worldMin':[min(v[i]for v in points)for i in range(3)],'worldMax':[max(v[i]for v in points)for i in range(3)],'armatures':[{'name':a.name,'scale':list(a.scale),'bones':list(a.data.bones.keys())}for a in arms],
 'actions':[{'name':a.name,'range':list(a.frame_range),'slots':[s.identifier for s in a.slots]}for a in bpy.data.actions], 'fps':bpy.context.scene.render.fps}
 reports.append(rec)
 print(json.dumps(rec),flush=True)
(folder/'inspection.json').write_text(json.dumps(reports,indent=2)+'\n')
