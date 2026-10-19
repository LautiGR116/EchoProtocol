"""Original rotary telephone with a separate handset. Metres; X width/-Y front/Z up.
One original asset, no imported media or gameplay. Reuses original
project geometry helpers from DomesticLighting, copied here for standalone rebuild.
"""
import bpy, os, json, math
from math import sin, cos, pi
from mathutils import Vector
BASE=os.path.dirname(os.path.abspath(__file__));OUT=os.path.join(BASE,'exports');PRE=os.path.join(BASE,'previews')
for p in [OUT,PRE]:os.makedirs(p,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
VALUES={
 'PH_Bakelite':{'color':(.034,.025,.019,1),'roughness':.31,'metallic':0},
 'PH_Rim':{'color':(.068,.050,.034,1),'roughness':.37,'metallic':0},
 'PH_Rubber':{'color':(.013,.015,.012,1),'roughness':.66,'metallic':0},
 'PH_Nickel':{'color':(.29,.27,.22,1),'roughness':.34,'metallic':.78},
 'PH_Ivory':{'color':(.66,.56,.37,1),'roughness':.57,'metallic':0},
 'PH_Ink':{'color':(.024,.031,.027,1),'roughness':.60,'metallic':0}}
mats={};parts=[];groups=[]
for n,d in VALUES.items():
 m=bpy.data.materials.new(n);m.use_nodes=True;m.diffuse_color=d['color'];p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=d['color'];p.inputs['Roughness'].default_value=d['roughness'];p.inputs['Metallic'].default_value=d['metallic'];mats[n]=m
 if n=='RO_Wood':
  coords=m.node_tree.nodes.new('ShaderNodeTexCoord');scale=m.node_tree.nodes.new('ShaderNodeVectorMath');scale.operation='MULTIPLY';scale.inputs[1].default_value=(1.4,65,5)
  t=m.node_tree.nodes.new('ShaderNodeTexNoise');t.inputs['Scale'].default_value=4;t.inputs['Detail'].default_value=3;t.inputs['Roughness'].default_value=.7
  m.node_tree.links.new(coords.outputs['Generated'],scale.inputs[0]);m.node_tree.links.new(scale.outputs['Vector'],t.inputs['Vector'])
  ramp=m.node_tree.nodes.new('ShaderNodeValToRGB');ramp.color_ramp.elements[0].position=.15;ramp.color_ramp.elements[0].color=(.10,.052,.020,1);ramp.color_ramp.elements[1].position=.83;ramp.color_ramp.elements[1].color=(.21,.129,.061,1)
  m.node_tree.links.new(t.outputs['Fac'],ramp.inputs['Fac']);m.node_tree.links.new(ramp.outputs['Color'],p.inputs['Base Color'])
  b=m.node_tree.nodes.new('ShaderNodeBump');b.inputs['Strength'].default_value=.10;b.inputs['Distance'].default_value=.00025;m.node_tree.links.new(t.outputs['Fac'],b.inputs['Height']);m.node_tree.links.new(b.outputs['Normal'],p.inputs['Normal'])
def add(o,mat):o.data.materials.append(mats[mat]);parts.append(o);return o
def mesh(n,v,f,mat,smooth=True):
 me=bpy.data.meshes.new(n);me.from_pydata(v,[],f);me.update();o=bpy.data.objects.new(n,me);bpy.context.collection.objects.link(o);add(o,mat)
 for p in me.polygons:p.use_smooth=smooth
 return o
def lathe(n,profile,mat,N=40):
 v=[(r*cos(i*2*pi/N),r*sin(i*2*pi/N),z) for r,z in profile for i in range(N)];f=[]
 for j in range(len(profile)-1):
  for i in range(N):a=j*N+i;b=j*N+(i+1)%N;f.append((a,b,b+N,a+N))
 f.extend([tuple(reversed(range(N))),tuple((len(profile)-1)*N+i for i in range(N))]);return mesh(n,v,f,mat)
def cyl(n,p,r,h,mat,axis=(0,0,1),N=24):
 bpy.ops.mesh.primitive_cylinder_add(vertices=N,radius=r,depth=h,location=p);o=bpy.context.object;o.name=n;o.rotation_euler=Vector(axis).to_track_quat('Z','Y').to_euler();add(o,mat)
 for f in o.data.polygons:f.use_smooth=len(f.vertices)==4
 return o
def sphere(n,p,s,mat):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=12,location=p);o=bpy.context.object;o.name=n;o.scale=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);add(o,mat)
 for f in o.data.polygons:f.use_smooth=True
 return o
def box(n,p,s,mat,b=.003):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=n;o.dimensions=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);add(o,mat)
 if b:
  m=o.modifiers.new('Rounded formed edge','BEVEL');m.width=b;m.segments=2;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=m.name)
  for f in o.data.polygons:f.use_smooth=True
  m=o.modifiers.new('Weighted normals','WEIGHTED_NORMAL');m.keep_sharp=True;bpy.ops.object.modifier_apply(modifier=m.name)
 return o
def tube(n,pts,r,mat,closed=False):
 cu=bpy.data.curves.new(n,'CURVE');cu.dimensions='3D';cu.resolution_u=1 if len(pts)>16 else 5;cu.bevel_depth=r;cu.bevel_resolution=1;sp=cu.splines.new('BEZIER');sp.bezier_points.add(len(pts)-1)
 for j,(b,p) in enumerate(zip(sp.bezier_points,pts)):
  p=Vector(p);prev=Vector(pts[(j-1)%len(pts)] if closed or j>0 else pts[j]);nxt=Vector(pts[(j+1)%len(pts)] if closed or j<len(pts)-1 else pts[j]);t=(nxt-prev)/6;left=p-t;right=p+t
  for k in range(3):left[k]=max(min(prev[k],p[k]),min(max(prev[k],p[k]),left[k]));right[k]=max(min(nxt[k],p[k]),min(max(nxt[k],p[k]),right[k]))
  b.co=p;b.handle_left_type='FREE';b.handle_right_type='FREE';b.handle_left=left;b.handle_right=right
 sp.use_cyclic_u=closed;o=bpy.data.objects.new(n,cu);bpy.context.collection.objects.link(o);bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o;bpy.ops.object.convert(target='MESH');o=bpy.context.object;add(o,mat)
 for f in o.data.polygons:f.use_smooth=True
 return o
def rod(n,a,b,r,mat):d=Vector(b)-Vector(a);return cyl(n,(Vector(a)+Vector(b))/2,r,d.length,mat,d)
def ring(n,r,z,thick,mat):return tube(n,[(r*cos(a),r*sin(a),z) for a in [i*2*pi/32 for i in range(32)]],thick,mat,True)
def finish_group(n):
 global parts
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name=n;scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(angle_limit=1.1519,island_margin=.012,scale_to_bounds=True);bpy.ops.object.mode_set(mode='OBJECT');o.data.uv_layers.active.name='UVMap';groups.append(o);parts=[];return o

roots=[];assets={}
def export_asset(name,markers,notes):
 global groups
 root=bpy.data.objects.new('EP_PH_'+name,None);bpy.context.collection.objects.link(root);roots.append(root)
 for o in groups:o.parent=root
 anchors={'ROOT':(0,0,0),'AXIS_X':(.1,0,0),'AXIS_Y':(0,0.1,0),'AXIS_Z':(0,0,.1),'FRONT_BASE':(0,0,0),'FRONT_TIP':(0,-.1,0)};anchors.update(markers);full={}
 for n,p in anchors.items():
  fn=name+'__'+n;o=bpy.data.objects.new(fn,None);bpy.context.collection.objects.link(o);o.parent=root;o.location=p;o.empty_display_type='PLAIN_AXES';o.empty_display_size=.025;full[fn]=list(p)
 bpy.ops.object.select_all(action='DESELECT')
 for o in [root]+list(root.children_recursive):o.select_set(True)
 bpy.context.view_layer.objects.active=root;relative='exports/EP_PH_'+name+'.fbx';bpy.ops.export_scene.fbx(filepath=os.path.join(BASE,relative),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH','EMPTY'},bake_anim=False)
 coords=[o.matrix_world@v.co for o in groups for v in o.data.vertices];bb=[[min(p[i] for p in coords),max(p[i] for p in coords)] for i in range(3)]
 assets[name]={'file':relative,'bounds_blender_xyz':bb,'dimensions_width_depth_height':[b-a for a,b in bb],'materials':sorted({m.name for o in groups for m in o.data.materials}),'mesh_objects':[{'name':o.name,'vertices':len(o.data.vertices),'triangles':sum(len(f.vertices)-2 for f in o.data.polygons),'uv_layers':[u.name for u in o.data.uv_layers]} for o in groups],'anchors_blender_xyz':full,'notes':notes}
 for o in [root]+list(root.children_recursive):o.hide_set(True);o.hide_render=True
 groups=[];print('EXPORTED',name,assets[name]['dimensions_width_depth_height'],flush=True)

from mathutils import Matrix
# Rounded wedge shell: small feet keep the real underside above the table.
for x in [-.102,.102]:
 for y in [-.077,.076]:cyl('Rubber foot',(x,y,.003),.011,.006,'PH_Rubber',N=16)
box('Base seam',(0,0,.013),(.282,.235,.022),'PH_Rubber',.011)
# Eight-point rounded profiles prevent a primitive box silhouette.
profiles=[(.141,.118,.019),(.144,.117,.039),(.125,.094,.093)]
v=[]
for w,d,z in profiles:
 for x,y in [(-w+.020,-d),(w-.020,-d),(w,-d+.020),(w,d-.020),(w-.020,d),(-w+.020,d),(-w,d-.020),(-w,-d+.020)]:
  v.append((x,y,z+(y+.094)*.20 if z>.08 else z))
f=[]
for j in range(2):
 for i in range(8):a=j*8+i;b=j*8+(i+1)%8;f.append((a,b,b+8,a+8))
f.extend([tuple(reversed(range(8))),tuple(16+i for i in range(8))]);o=mesh('Bakelite wedge body',v,f,'PH_Bakelite',True)
m=o.modifiers.new('Soft molded corners','BEVEL');m.width=.008;m.segments=3;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=m.name)
m=o.modifiers.new('Molded normals','WEIGHTED_NORMAL');m.keep_sharp=True;bpy.ops.object.modifier_apply(modifier=m.name)
for x in [-.097,.097]:
 box('Cradle support',(x,.069,.139),(.029,.028,.051),'PH_Bakelite',.006)
 cyl('Depressed hook button',(x,.054,.158),.006,.018,'PH_Nickel',N=16)
# Dial follows the rising face; real annulus and ten recessed finger wells.
rot=Matrix.Rotation(math.radians(12),4,'X');origin=Vector((0,-.025,.114))
def dp(p):return origin+rot@Vector(p)
def disk(n,p,r,h,mat,N=40):return cyl(n,dp(p),r,h,mat,rot@Vector((0,0,1)),N)
def circle(n,c,r,thick,mat):return tube(n,[dp(Vector(c)+Vector((r*cos(i*2*pi/48),r*sin(i*2*pi/48),0))) for i in range(48)],thick,mat,True)
disk('Dial mounting plate',(0,0,0),.085,.008,'PH_Rim');disk('Dial ivory face',(0,0,.006),.079,.003,'PH_Ivory')
circle('Dial outer polished rim',(0,0,.010),.078,.002,'PH_Nickel')
circle('Dial centre bezel',(0,0,.012),.031,.0025,'PH_Nickel');disk('Dial label',(0,0,.009),.029,.004,'PH_Ivory')
# Recessed dark finger cups with an individual formed rim are more legible than solid discs.
for i in range(10):
 a=math.radians(25+i*29);x,y=.058*cos(a),.058*sin(a)
 disk('Finger well',(x,y,.010),.010,.004,'PH_Rubber',20);circle('Finger ring',(x,y,.013),.0105,.0018,'PH_Nickel')
 # Original stroke numerals, no external fonts. They are physical markings outside the well.
 patterns={'0':['ab','bc','cd','de','ef','fa'],'1':['bc','cd'],'2':['ab','bc','fg','ge','ed'],'3':['ab','bc','fg','cd','de'],'4':['fa','fg','bc','cd'],'5':['ab','af','fg','gc','cd','de'],'6':['ab','af','fg','ge','ed','dc'],'7':['ab','bc','cd'],'8':['ab','bc','cd','de','ef','fa','fg'],'9':['ab','bc','cd','fg','fa']}
 points={'a':(-1,2),'b':(1,2),'c':(1,0),'d':(1,-2),'e':(-1,-2),'f':(-1,0),'g':(1,0)}
 cx,cy=.073*cos(a),.073*sin(a)
 for seg in patterns[str((i+1)%10)]:
  pts=[]
  for k in seg:
   u,w=points[k];pts.append(dp((cx+u*.0014,cy+w*.0014,.013)))
  rod('Stamped dial numeral',*pts,.00045,'PH_Ink')
# Station label and the metal finger stop are authored, brandless geometry.
box('Finger stop foot',dp((.070,-.029,.010)),(.016,.008,.004),'PH_Nickel',.001)
rod('Finger stop',dp((.070,-.027,.013)),dp((.062,-.022,.029)),.003,'PH_Nickel')
# Number field on the centre label: restrained ink bars rather than unreadable invented branding.
for y,w in [(.005,.027),(-.003,.017),(-.009,.023)]:rod('Label ink',dp((-w/2,y,.012)),dp((w/2,y,.012)),.0005,'PH_Ink')
# Side cable socket. Coiled live cord will be attached explicitly by Unity.
cyl('Cord grommet',(-.137,.050,.061),.007,.012,'PH_Rubber',(1,0,0),20)
finish_group('TelephoneBody')
# Handset: a formed bridge, distinct speaker and mouthpiece caps, underside perforations.
pivot=Vector((0,.065,.194))
for x in [-.112,.112]:
 sphere('Handset ear body',(x,.065,.174),(.036,.038,.041),'PH_Bakelite')
 cyl('Threaded receiver collar',(x,.065,.148),.034,.018,'PH_Rim',N=32)
 cyl('Receiver acoustic face',(x,.065,.137),.029,.003,'PH_Rubber',N=32)
 for j in range(7):
  a=j*2*pi/7;cyl('Acoustic perforation',(x+.018*cos(a),.065+.018*sin(a),.1348),.002,.001,'PH_Ink',N=8)
box('Formed handset bridge',(0,.065,.208),(.205,.034,.031),'PH_Bakelite',.012)
for x in [-.077,.077]:sphere('Bridge blended shoulder',(x,.065,.202),(.028,.020,.028),'PH_Bakelite')
cyl('Handset cord ferrule',(-.144,.065,.181),.0055,.011,'PH_Rubber',(1,0,0),16)
hand=finish_group('TelephoneHandset');scene.cursor.location=pivot;bpy.context.view_layer.objects.active=hand;bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
export_asset('RotaryTelephone',{'TABLE_CONTACT':(0,0,0),'HANDSET_REST':tuple(pivot),'HANDSET_OFFHOOK':(-.232,-.018,.063),'CORD_BASE':(-.144,.050,.061),'CORD_HANDSET_REST':(-.151,.065,.181)},'Original brandless rotary phone. Body and handset separate. Handset pivot at HANDSET_REST; rotate +90deg sourceZ and place at HANDSET_OFFHOOK for tabletop rest. Runtime cord is separate and follows both physical endpoints. No dial interaction or voice implied.')
triangles=sum(m['triangles'] for a in assets.values() for m in a['mesh_objects'])
manifest={'date':'2026-10-01','authorship':'Original project geometry and material values; no third-party model, texture, font or service.','axis_contract':'Source Xwidth,-Yfront,Zup. FBX -Z/+Y; Unity negate imported root X once, retain import rotation and scale. Corrected(X,Z,-Y).','materials':VALUES,'total_triangles':triangles,'assets':assets}
json.dump(manifest,open(os.path.join(BASE,'manifest.json'),'w'),indent=2)
for o in [roots[0]]+list(roots[0].children_recursive):o.hide_set(False);o.hide_render=False
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BASE,'EchoTelephone.blend'))
# Studio only, no exported light or fake table.
scene.render.engine='CYCLES';scene.cycles.samples=32;scene.cycles.use_denoising=True;scene.render.resolution_x=1200;scene.render.resolution_y=900;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX';scene.world.color=(.10,.10,.10)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.001));ground=bpy.context.object;ground.data.materials.append(mats['PH_Rim'])
for n,p,power,size,col in [('Key',(-.6,-.9,1),65,1,(1,.86,.68)),('Fill',(.7,-.3,.8),35,1,(.67,.8,1)),('Rim',(0,.8,1.1),45,.7,(1,.94,.8))]:
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.size=size;d.color=col;o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=p;o.rotation_euler=(Vector((0,0,.1))-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;scene.camera=cam;cam.data.lens=56
for name,offhook in [('Telephone-onhook',False),('Telephone-offhook',True)]:
 if offhook:hand.location=(-.232,-.018,.063);hand.rotation_euler.z=pi/2
 cam.location=(-.56,-.69,.53);cam.rotation_euler=(Vector((-.055,0,.08))-cam.location).to_track_quat('-Z','Y').to_euler();scene.render.filepath=os.path.join(PRE,name+'.png');bpy.ops.render.render(write_still=True)
print('TELEPHONE COMPLETE',triangles,flush=True)
