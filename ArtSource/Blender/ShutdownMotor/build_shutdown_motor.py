"""Original Shutdown derivative of project PressurePlant MotorSkid.

Only writes this directory. No external files/services/assets. Helpers and the
unchanged original motor construction are copied from PressurePlant, followed
by a true annular hood and a separate authored matte impeller wheel. No runtime
animation, collider, light, text, audio or gameplay is exported.
"""
import bpy, math, os, json, hashlib
from pathlib import Path
from mathutils import Vector
from math import sin, cos, pi
BASE=Path(__file__).resolve().parent
OUT=BASE/'exports'; PRE=BASE/'previews'
for p in (OUT,PRE):p.mkdir(exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
SOURCE_HASH='e19bd9934af9686c7a390814f10b881a7c1cfd7a4c96b15a837cd7603917c421'
MATERIALS={
 'PP_ServiceEnamel':{'color':(.285,.305,.271,1),'roughness':.57,'metallic':.12},
 'PP_ReplacementPanel':{'color':(.31,.321,.291,1),'roughness':.63,'metallic':.10},
 'PP_ReceiverPaint':{'color':(.15,.209,.219,1),'roughness':.54,'metallic':.13},
 'PP_MotorPaint':{'color':(.095,.137,.143,1),'roughness':.51,'metallic':.15},
 'PP_PipePaint':{'color':(.048,.061,.056,1),'roughness':.56,'metallic':.24},
 'PP_CastIron':{'color':(.078,.094,.086,1),'roughness':.60,'metallic':.7},
 'PP_MachinedSteel':{'color':(.25,.28,.26,1),'roughness':.39,'metallic':.8},
 'PP_FastenerZinc':{'color':(.39,.405,.37,1),'roughness':.38,'metallic':.82},
 'PP_Rubber':{'color':(.016,.021,.018,1),'roughness':.87,'metallic':0},
 'PP_Recess':{'color':(.021,.027,.024,1),'roughness':.89,'metallic':0},
 'PP_Oxidation':{'color':(.17,.079,.034,1),'roughness':.87,'metallic':.05},
 'PP_PlateIvory':{'color':(.49,.477,.373,1),'roughness':.67,'metallic':.05}}

MATERIALS['SD_RotorMatte']={'color':(.078,.090,.080,1),'roughness':.77,'metallic':.14}
MATERIALS['SD_RotorPatina']={'color':(.054,.067,.058,1),'roughness':.84,'metallic':.08}
mats={}
for name,data in MATERIALS.items():
 m=bpy.data.materials.new(name);m.use_nodes=True;m.diffuse_color=data['color'];p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=data['color'];p.inputs['Roughness'].default_value=data['roughness'];p.inputs['Metallic'].default_value=data['metallic'];mats[name]=m
parts=[];roots=[];manifest={}
def register(o,mat):
 o.data.materials.append(mats[mat]);parts.append(o);return o
def normals(o):
 for f in o.data.polygons:f.use_smooth=True
 mod=o.modifiers.new('Weighted hard surface normals','WEIGHTED_NORMAL');mod.keep_sharp=True;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
def cube(n,p,s,mat='PP_ServiceEnamel',bevel=.006,rot=(0,0,0)):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p,rotation=rot);o=bpy.context.object;o.name=n;o.dimensions=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);register(o,mat)
 if bevel:
  b=o.modifiers.new('Manufactured edge','BEVEL');b.width=bevel;b.segments=2;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=b.name);normals(o)
 return o
def cyl(n,p,r,h,mat='PP_MachinedSteel',axis=(0,0,1),verts=32,bevel=0):
 bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=r,depth=h,location=p);o=bpy.context.object;o.name=n;o.rotation_euler=Vector(axis).to_track_quat('Z','Y').to_euler();register(o,mat)
 for f in o.data.polygons:f.use_smooth=len(f.vertices)==4
 if bevel:
  b=o.modifiers.new('Cast edge','BEVEL');b.width=bevel;b.segments=2;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=b.name);normals(o)
 return o
def rod(n,a,b,r,mat='PP_PipePaint',verts=24):
 d=Vector(b)-Vector(a);return cyl(n,(Vector(a)+Vector(b))/2,r,d.length,mat,d,verts)
def mesh(n,v,f,mat,smooth=True):
 me=bpy.data.meshes.new(n);me.from_pydata(v,[],f);me.update();o=bpy.data.objects.new(n,me);bpy.context.collection.objects.link(o);register(o,mat)
 for face in me.polygons:face.use_smooth=smooth
 return o
def lathe(n,profile,p=(0,0,0),mat='PP_ReceiverPaint',segments=48):
 v=[];f=[]
 for r,z in profile:
  for i in range(segments):a=i*2*pi/segments;v.append((p[0]+r*cos(a),p[1]+r*sin(a),p[2]+z))
 for j in range(len(profile)-1):
  for i in range(segments):a=j*segments+i;b=j*segments+(i+1)%segments;f.append((a,b,b+segments,a+segments))
 f+=[tuple(reversed(range(segments))),tuple((len(profile)-1)*segments+i for i in range(segments))]
 return mesh(n,v,f,mat)
def tube(n,pts,r=.09,wall=.008,mat='PP_PipePaint',segments=24):
 # Real inner bore with end annuli; no black disk posing as a hole.
 pts=[Vector(p) for p in pts];v=[];f=[]
 frames=[];previous_u=None
 for j,p in enumerate(pts):
  t=(pts[min(j+1,len(pts)-1)]-pts[max(0,j-1)]).normalized()
  if previous_u is None:
   guide=Vector((0,0,1)) if abs(t.z)<.95 else Vector((0,1,0));u=t.cross(guide).normalized()
  else:u=(previous_u-t*previous_u.dot(t)).normalized()
  w=t.cross(u).normalized();frames.append((u,w));previous_u=u
 for rad in [r,r-wall]:
  for p,(u,w) in zip(pts,frames):
   for k in range(segments):a=k*2*pi/segments;v.append(tuple(p+rad*(cos(a)*u+sin(a)*w)))
 count=len(pts)*segments
 for side in [0,1]:
  off=side*count
  for j in range(len(pts)-1):
   for k in range(segments):a=off+j*segments+k;b=off+j*segments+(k+1)%segments;face=(a,b,b+segments,a+segments);f.append(tuple(reversed(face)) if side else face)
 for end in [0,len(pts)-1]:
  for k in range(segments):a=end*segments+k;b=end*segments+(k+1)%segments;face=(a,a+count,b+count,b);f.append(tuple(reversed(face)) if end else face)
 o=mesh(n,v,f,mat)
 for face in list(o.data.polygons)[-segments*2:]:face.use_smooth=False
 return o
def torus(n,p,r,t,mat='PP_MachinedSteel',axis=(0,0,1),major=40,minor=8):
 bpy.ops.mesh.primitive_torus_add(major_radius=r,minor_radius=t,major_segments=major,minor_segments=minor,location=p);o=bpy.context.object;o.name=n;o.rotation_euler=Vector(axis).to_track_quat('Z','Y').to_euler();register(o,mat)
 for f in o.data.polygons:f.use_smooth=True
 return o
def bolt(n,p,axis=(0,0,1),r=.012,head=.010,mat='PP_FastenerZinc'):
 a=Vector(axis).normalized();p=Vector(p);cyl(n+' washer',p-a*.002,r*1.40,.003,mat,a,16);return cyl(n+' hex',p+a*head*.5,r,head,mat,a,6,.0007)
def flange(n,p,axis=(1,0,0),r=.09,outer=.145,thickness=.028,bolts=6,mat='PP_MachinedSteel'):
 a=Vector(axis).normalized();p=Vector(p);tube(n+' ring',[p-a*thickness/2,p+a*thickness/2],outer,outer-(r-.007),mat,32)
 guide=Vector((0,0,1)) if abs(a.z)<.95 else Vector((0,1,0));u=a.cross(guide).normalized();v=a.cross(u).normalized()
 for i in range(bolts):ang=(i+.5)*2*pi/bolts;bolt(n+' bolt',p+a*(thickness*.5)+(.5*(outer+r))*(u*cos(ang)+v*sin(ang)),a,r=.0085,head=.008)
def plate(n,p,s,normal=(0,-1,0)):
 # Riveted rating blank, no generated lettering or fake legibility.
 o=cube(n,p,s,'PP_PlateIvory',.002)
 for xx in [-.5,.5]:
  for zz in [-.5,.5]:cyl(n+' rivet',Vector(p)+Vector((xx*(s[0]-.018),-.5*s[1]-.001,zz*(s[2]-.013))),.0025,.003,'PP_MachinedSteel',normal,8)
 return o
def join(name,items):
 bpy.ops.object.select_all(action='DESELECT')
 for o in items:o.select_set(True)
 bpy.context.view_layer.objects.active=items[0];bpy.ops.object.join();o=bpy.context.object;o.name=name;scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 # Packed, non-overlapping UVs per final mesh, including custom tube/lathe meshes.
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(angle_limit=1.1519,island_margin=.015,correct_aspect=True,scale_to_bounds=True);bpy.ops.object.mode_set(mode='OBJECT')
 o.data.uv_layers.active.name='UVMap';return o

def open_hood():
 # Closed annular section with a bevelled lip and actual continuous bore.
 # X/r profile is traversed once; no end face covers the centre.
 profile=[(-.7125,.232),(-.703,.245),(-.607,.245),(-.5975,.234),
          (-.5975,.190),(-.702,.190),(-.7125,.200)]
 N=64;v=[];f=[]
 for x,r in profile:
  for k in range(N):a=k*2*pi/N;v.append((x,r*cos(a),.535+r*sin(a)))
 for j in range(len(profile)):
  for k in range(N):a=j*N+k;b=j*N+(k+1)%N;c=((j+1)%len(profile))*N+(k+1)%N;d=((j+1)%len(profile))*N+k;f.append((a,b,c,d))
 o=mesh('Open cast fan hood',v,f,'PP_MotorPaint')
 import bmesh
 bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(o.data);bm.free()
 # Visible front bevel retains the existing silhouette. Inner bore stays open.
 return o

def make_rotor():
 # Matte front shroud of a cooling impeller, a broad pressed depression rather
 # than bright isolated blades. A shallow, asymmetric surface catches local
 # light at rest, with low contrast when rapidly moving behind the fixed guard.
 N=40; rs=[0,.035,.065,.115,.151,.160]
 v=[];f=[]
 for r in rs:
  for k in range(N):
   a=k*2*pi/N
   angular=max(0,cos(a-.65))**2
   radial=max(0,1-((r-.101)/.060)**2)
   recess=.004*angular*radial
   rim=(.001 if r>=.157 else 0)
   v.append((-.685+recess-rim,r*cos(a),.535+r*sin(a)))
 front=len(v)
 for r in rs:
  for k in range(N):a=k*2*pi/N;v.append((-.679,r*cos(a),.535+r*sin(a)))
 for side in (0,1):
  off=side*front
  for j in range(len(rs)-1):
   for k in range(N):
    a=off+j*N+k;b=off+j*N+(k+1)%N;c=b+N;d=a+N
    f.append((d,c,b,a) if side==0 else (a,b,c,d))
 for k in range(N):a=(len(rs)-1)*N+k;b=(len(rs)-1)*N+(k+1)%N;f.append((a,b,b+front,a+front))
 o=mesh('Pressed impeller wheel',v,f,'SD_RotorMatte')
 # Merge only the repeated axis vertices, keeping a closed manifold disk.
 import bmesh
 bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.remove_doubles(bm,verts=bm.verts,dist=1e-6);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(o.data);bm.free()
 # One broad dull patina sector, without a bright blade or a repeated stripe.
 # This is an authored surface variation, not a light or animation effect.
 o.data.materials.append(mats['SD_RotorPatina'])
 for face in o.data.polygons:
  c=face.center; a=math.atan2(c.z-.535,c.y) % (2*pi)
  if c.x < -.680 and math.hypot(c.y,c.z-.535)>.045 and .16<a<1.47:
   face.material_index=1
 cyl('Impeller hub',(-.688,0,.535),.033,.016,'SD_RotorMatte',(-1,0,0),24)
 cyl('Recessed shaft cap',(-.697,0,.535),.013,.003,'PP_Recess',(-1,0,0),16)
 return o
# Compact pump motor: evolved from Campaign motor grammar, original new mesh.
cube('Skid pan',(0,0,.055),(1.52,.80,.11),'PP_CastIron',.012)
for y in [-.29,.29]:cube('Structural runner',(0,y,.16),(1.37,.09,.13),'PP_MachinedSteel',.005)
for x in [-.61,.61]:
 for y in [-.29,.29]:bolt('Skid foundation bolt',(x,y,.229),r=.013,head=.015)
for x in [-.48,-.03]:
 cube('Motor mounting foot',(x,0,.274),(.16,.40,.10),'PP_MotorPaint',.008)
 for y in [-.14,.14]:bolt('Motor mounting bolt',(x,y,.33),r=.011,head=.013)
cyl('Cast motor body',(-.285,0,.535),.219,.64,'PP_MotorPaint',(1,0,0),48,.010)
# Longitudinal cooling fins parallel to rotor axis, unlike repeated torus toy coils.
for i in range(18):
 a=i*2*pi/18;y=.232*cos(a);z=.535+.232*sin(a)
 o=cube('Motor cooling vane',(-.285,y,z),(.52,.039,.014),'PP_MotorPaint',.003);o.rotation_euler.x=a
# Real open annular hood, no solid cap hiding the rotor.
open_hood()
cyl('Motor cavity backplane',(-.630,0,.535),.188,.006,'PP_Recess',(-1,0,0),48)
for k in range(-4,5):
 z=.535+k*.033;extent=math.sqrt(max(.001,.212**2-(k*.033)**2));rod('Fan guard strip',(-.723,-extent,z),(-.723,extent,z),.006,'PP_MachinedSteel',12)
torus('Fixed guard retaining ring',(-.723,0,.535),.212,.007,'PP_CastIron',(1,0,0),48,8)
for a in [pi*.25,pi*.75,pi*1.25,pi*1.75]:
 cyl('Fixed guard standoff',(-.718,.212*cos(a),.535+.212*sin(a)),.005,.024,'PP_CastIron',(1,0,0),12)
cyl('Front motor end',(.07,0,.535),.225,.07,'PP_MotorPaint',(1,0,0),48,.006)
cyl('Coupling guard',(.20,0,.535),.13,.23,'PP_MachinedSteel',(1,0,0),32)
for x in [.135,.19,.245]:torus('Coupling guard collar',(x,0,.535),.132,.006,'PP_CastIron',(1,0,0),32,6)
cube('Pump housing foot',(.445,0,.275),(.34,.31,.10),'PP_MotorPaint',.009)
cyl('Pump volute',(.445,0,.535),.245,.25,'PP_MotorPaint',(1,0,0),48,.009)
cyl('Pump casing cover',(.585,0,.535),.224,.047,'PP_MotorPaint',(1,0,0),48,.005)
for i in range(6):
 a=(i+.5)*pi/3;bolt('Pump cover bolt',(.614,.187*cos(a),.535+.187*sin(a)),(1,0,0),.010,.012)
tube('Pump axial inlet',[(.59,0,.535),(.79,0,.535)],.09,.008,'PP_MotorPaint');flange('Pump inlet',(.759,0,.535),(1,0,0))
tube('Pump upper discharge',[(.41,0,.70),(.41,0,1.01)],.09,.008,'PP_MotorPaint');flange('Pump discharge',(.41,0,.981),(0,0,1))
cube('Motor electrical box',(-.31,0,.822),(.26,.24,.14),'PP_MotorPaint',.018)
cube('Terminal box lid',(-.31,0,.902),(.28,.257,.026),'PP_MotorPaint',.007)
for x in [-.41,-.21]:
 for y in [-.086,.086]:bolt('Terminal lid screw',(x,y,.918),r=.0055,head=.005)
tube('Short wiring conduit',[(-.32,.13,.82),(-.32,.20,.82),(-.32,.26,.76),(-.32,.26,.22)],.013,.003,'PP_Rubber',12)
plate('Motor rating plate',(-.30,-.263,.543),(.23,.012,.091))

static=join('ShutdownMotorStatic',parts.copy());parts=[]
make_rotor();rotor=join('ShutdownRotor',parts.copy());parts=[]
scene.cursor.location=(-.682,0,.535);bpy.context.view_layer.objects.active=rotor
bpy.ops.object.select_all(action='DESELECT');rotor.select_set(True);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
root=bpy.data.objects.new('EP_SD_MotorSkid',None);bpy.context.collection.objects.link(root)
for o in (static,rotor):o.parent=root
anchors={'ROOT':(0,0,0),'AXIS_X':(.1,0,0),'AXIS_Y':(0,.1,0),'AXIS_Z':(0,0,.1),
 'ROTOR_PIVOT':(-.682,0,.535),'ROTOR_AXIS_POS':(-.582,0,.535),
 'ROTOR_REFERENCE':(-.682,.160,.535),
 'AXIAL_INLET':(.79,0,.535),'UPPER_DISCHARGE':(.41,0,1.01)}
for key,p in anchors.items():
 o=bpy.data.objects.new('MotorSkid__'+key,None);bpy.context.collection.objects.link(o);o.parent=root;o.location=p;o.empty_display_type='PLAIN_AXES';o.empty_display_size=.02
objects=[root]+list(root.children_recursive)
bpy.context.view_layer.update();bpy.ops.object.select_all(action='DESELECT')
for o in objects:o.select_set(True)
bpy.context.view_layer.objects.active=root
fbx=OUT/'EP_SD_MotorSkid.fbx'
bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH','EMPTY'},bake_anim=False,use_mesh_modifiers=True,path_mode='RELATIVE')
coords=[o.matrix_world@v.co for o in (static,rotor) for v in o.data.vertices]
bounds=[[min(p[i] for p in coords),max(p[i] for p in coords)] for i in range(3)]
meshes=[{'name':o.name,'vertices':len(o.data.vertices),'triangles':sum(len(f.vertices)-2 for f in o.data.polygons),'pivot_blender_xyz':list(o.matrix_world.translation),'uv_layers':[u.name for u in o.data.uv_layers]} for o in (static,rotor)]
asset={'file':'exports/EP_SD_MotorSkid.fbx','bounds_blender_xyz':bounds,'materials':sorted({m.name for o in (static,rotor) for m in o.data.materials}),'mesh_objects':meshes,'anchors_blender_xyz':{'MotorSkid__'+k:list(v) for k,v in anchors.items()},
 'ports_blender_xyz':[{'name':'axial inlet','position':[.79,0,.535],'outward':[1,0,0],'bore_diameter':.164},{'name':'upper discharge','position':[.41,0,1.01],'outward':[0,0,1],'bore_diameter':.164}],
 'animation':{'mesh':'ShutdownRotor','pivot_blender_xyz':[-.682,0,.535],'axis_blender_xyz':[1,0,0],'candidate_degrees_per_second':1440,'status':'Provisional presentation speed, not simulated motor RPM; root must judge real camera aliasing, slowdown and comfort.'},
 'notes':'Original existing MotorSkid body/skid/ports rebuilt exactly; only original capped rear hood and disk replaced. Fixed guard belongs to ShutdownMotorStatic. The matte wheel is separate and the inlet/discharge stay connected. No collider, light, text, audio or runtime animation exported.'}
manifest={'date':'2026-10-01','authorship':'Original project derivative of PressurePlant. Construction helpers and original unchanged MotorSkid geometry copied; true annular hood and rotor newly authored here. No external asset, service or purchase.',
 'source_pressureplant_builder_sha256':SOURCE_HASH,
 'axis_contract':'Source X shaft axis, Z up. FBX -Z/+Y. Prior importer maps(-X,Z,-Y); apply the same corrected root X reflection used by PressurePlant, preserving import rotation/scale. Corrected assembly maps(X,Z,-Y). Markers are authoritative.',
 'materials':{n:MATERIALS[n] for n in asset['materials']},'assets':{'MotorSkid':asset},'total_triangles':sum(m['triangles'] for m in meshes),'verification_limit':'Source geometry and Blender previews only; Unity placement/materials/motion and human experience remain pending.'}
(BASE/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
bpy.ops.wm.save_as_mainfile(filepath=str(BASE/'EchoShutdownMotor.blend'))
print('EXPORTED',json.dumps({'triangles':manifest['total_triangles'],'bounds':bounds,'meshes':meshes}),flush=True)

# Small studio previews, not Unity or in-game lighting evidence.
scene.render.engine='CYCLES';scene.cycles.samples=20;scene.cycles.use_denoising=True
scene.render.resolution_x=1100;scene.render.resolution_y=800;scene.render.resolution_percentage=100
scene.world.color=(.025,.025,.025);scene.view_settings.view_transform='AgX'
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.004));ground=bpy.context.object;ground.name='Preview only floor'
gm=bpy.data.materials.new('Preview floor');gm.use_nodes=True;gm.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.055,.065,.060,1);gm.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.86;ground.data.materials.append(gm)
for name,p,power,size,color in [('Cool key',(-3,-2.5,4),550,3,(.72,.86,1)),('Rear',(2,2,3),500,3,(.95,.87,.72)),('Cavity fill',(-2,.8,1.2),100,2,(.8,.9,.85))]:
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.size=size;d.color=color;o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o);o.location=p;o.rotation_euler=(Vector((0,0,.5))-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();camera=bpy.context.object;scene.camera=camera

def render(name,p,target,lens=48):
 camera.location=p;camera.rotation_euler=(Vector(target)-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.lens=lens;scene.render.filepath=str(PRE/(name+'.png'));bpy.ops.render.render(write_still=True)
render('MotorSkid-overview',(-1.9,-2.3,1.5),(0,0,.47),48)
render('Rotor-close',(-1.23,-.34,.79),(-.68,0,.535),57)
rotor.rotation_euler.x=math.radians(95)
render('Rotor-other-rest',(-1.23,-.34,.79),(-.68,0,.535),57)
rotor.rotation_euler.x=0
print('SHUTDOWN MOTOR COMPLETE',flush=True)
