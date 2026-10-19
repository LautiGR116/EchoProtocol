"""Original derivative of the project-owned adult surface; fixed display geometry only."""
import bpy,math,json,hashlib
from pathlib import Path
from mathutils import Vector
root=Path('/Users/lautigr116/EchoProtocol');out=root/'ArtSource/Blender/EncounterArt';(out/'exports').mkdir(exist_ok=True)
source=root/'ArtSource/Blender/OverhaulFigure/EchoAdultFigure.blend';bpy.ops.wm.open_mainfile(filepath=str(source));o=bpy.data.objects['EP_AdultRecordedSurface']
# Relax the straight sleeve cylinder and make tailored cloth relief catch oblique practicals.
changed=0
for v in o.data.vertices:
 p=v.co;old=p.copy();x,y,z=p;side=1 if x>0 else -1
 if .99<z<1.40 and abs(x)>.19:
  t=max(0,min(1,(1.40-z)/.40));p.x-=side*.014*math.sin(t*math.pi*.75);p.y+=.010*t*(1 if side<0 else -.4)
 if .84<z<.99 and abs(x)>.20:p.x-=side*.009;p.y+=.009*(1 if side<0 else -.4)
 if .97<z<1.36 and abs(x)<.19 and y<-.06:
  p.y-=.0019*math.sin(z*57+abs(x)*21)*math.exp(-((z-1.15)/.17)**2)
 if .12<z<.60 and abs(x)<.19:p.y+=.0017*math.sin(z*76+x*16)*math.exp(-((z-.42)/.18)**2)
 if z>1.52:p.z=1.52+(z-1.52)*.99
 if (p-old).length>.00001:changed+=1
# Neutral fixed eye/brow relief avoids the featureless display-doll face.
# These are static authored surface cues, never eye tracking or recorded expressions.
def head_pose(point):
 p=Vector(point)-Vector((.009,.005,1.50));a=.032;y=p.y*math.cos(a)-p.z*math.sin(a);z=p.y*math.sin(a)+p.z*math.cos(a);p.y=y;p.z=z
 a=-.035;x=p.x*math.cos(a)-p.y*math.sin(a);y=p.x*math.sin(a)+p.y*math.cos(a);p.x=x;p.y=y;p+=Vector((.009,.005,1.50));p.z=1.52+(p.z-1.52)*.99;return p
pieces=[]
for side in [-1,1]:
 for name,point,size in [('Neutral eye inset',(side*.031,-.069,1.636),(.010,.0022,.0036)),('Quiet brow plane',(side*.031,-.076,1.651),(.017,.0018,.0016))]:
  bpy.ops.mesh.primitive_uv_sphere_add(segments=20,ring_count=10,location=head_pose(point));e=bpy.context.object;e.name=name;e.scale=size;e.data.materials.append(bpy.data.materials['FIG_Hair']);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
  for poly in e.data.polygons:poly.use_smooth=True
  pieces.append(e)
bpy.ops.object.select_all(action='DESELECT');o.select_set(True)
for e in pieces:e.select_set(True)
bpy.context.view_layer.objects.active=o;bpy.ops.object.join()
# Named slots retained: hair, skin, cloth, hardware and footwear are distinct in Unity.
o.name='EP_WitnessSurface';bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
bpy.ops.export_scene.fbx(filepath=str(out/'exports/EP_WitnessSurface.fbx'),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH'},bake_anim=False)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'EchoWitnessSurface.blend'))
bounds=[[min(v.co[k] for v in o.data.vertices),max(v.co[k] for v in o.data.vertices)] for k in range(3)]
d={'date':'2026-10-02','authorship':'Project-owned procedural adult; derivative of OverhaulFigure. No real-person likeness or third-party model.','sourceSha256':hashlib.sha256(source.read_bytes()).hexdigest(),'model':'exports/EP_WitnessSurface.fbx','vertices':len(o.data.vertices),'triangles':sum(len(p.vertices)-2 for p in o.data.polygons),'modifiedVertices':changed,'boundsXYZ':bounds,'materials':[m.name for m in o.data.materials],'rig':False,'animations':False,'purpose':'Fixed display body, never a recording of appearance or articulation; sole recorded motion is original root position/body yaw.'}
(out/'manifest.json').write_text(json.dumps(d,indent=2)+'\n');print('WITNESS MODEL',d)
