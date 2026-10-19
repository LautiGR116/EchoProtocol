"""Original administrative review furniture. Metres; X width/-Y front/Z up.
Three independent original assets, no imported media or gameplay. Reuses original
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
 'RO_Wood':{'color':(.16,.091,.043,1),'roughness':.56,'metallic':0},
 'RO_WoodEdge':{'color':(.105,.063,.032,1),'roughness':.55,'metallic':0},
 'RO_Enamel':{'color':(.075,.098,.080,1),'roughness':.50,'metallic':.32},
 'RO_WornMetal':{'color':(.20,.22,.197,1),'roughness':.43,'metallic':.76},
 'RO_DrawerFace':{'color':(.235,.252,.211,1),'roughness':.60,'metallic':.12},
 'RO_DarkRubber':{'color':(.012,.016,.012,1),'roughness':.78,'metallic':0},
 'RO_Bakelite':{'color':(.039,.034,.023,1),'roughness':.43,'metallic':0},
 'RO_MonitorEnamel':{'color':(.23,.26,.224,1),'roughness':.53,'metallic':.15},
 'RO_BezelDark':{'color':(.025,.034,.029,1),'roughness':.59,'metallic':0},
 'RO_LabelBlank':{'color':(.47,.448,.348,1),'roughness':.86,'metallic':0},
 'RO_LampDiffuser':{'color':(.70,.652,.49,1),'roughness':.78,'metallic':0},
 'RO_Brass':{'color':(.26,.182,.085,1),'roughness':.44,'metallic':.73}}
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
 root=bpy.data.objects.new('EP_RO_'+name,None);bpy.context.collection.objects.link(root);roots.append(root)
 for o in groups:o.parent=root
 anchors={'ROOT':(0,0,0),'AXIS_X':(.1,0,0),'AXIS_Y':(0,0.1,0),'AXIS_Z':(0,0,.1),'FRONT_BASE':(0,0,0),'FRONT_TIP':(0,-.1,0)};anchors.update(markers);full={}
 for n,p in anchors.items():
  fn=name+'__'+n;o=bpy.data.objects.new(fn,None);bpy.context.collection.objects.link(o);o.parent=root;o.location=p;o.empty_display_type='PLAIN_AXES';o.empty_display_size=.025;full[fn]=list(p)
 bpy.ops.object.select_all(action='DESELECT')
 for o in [root]+list(root.children_recursive):o.select_set(True)
 bpy.context.view_layer.objects.active=root;relative='exports/EP_RO_'+name+'.fbx';bpy.ops.export_scene.fbx(filepath=os.path.join(BASE,relative),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH','EMPTY'},bake_anim=False)
 coords=[o.matrix_world@v.co for o in groups for v in o.data.vertices];bb=[[min(p[i] for p in coords),max(p[i] for p in coords)] for i in range(3)]
 assets[name]={'file':relative,'bounds_blender_xyz':bb,'dimensions_width_depth_height':[b-a for a,b in bb],'materials':sorted({m.name for o in groups for m in o.data.materials}),'mesh_objects':[{'name':o.name,'vertices':len(o.data.vertices),'triangles':sum(len(f.vertices)-2 for f in o.data.polygons),'uv_layers':[u.name for u in o.data.uv_layers]} for o in groups],'anchors_blender_xyz':full,'notes':notes}
 for o in [root]+list(root.children_recursive):o.hide_set(True);o.hide_render=True
 groups=[];print('EXPORTED',name,assets[name]['dimensions_width_depth_height'],flush=True)

# Review desk: a real .760 m working height, open knee bay, suspended side drawers.
box('Solid used timber worktop',(0,0,.741),(1.65,.78,.038),'RO_Wood',.007)
box('Hardwood front lipping',(0,-.386,.736),(1.638,.012,.038),'RO_WoodEdge',.004)
box('Rear top lipping',(0,.386,.736),(1.638,.012,.038),'RO_WoodEdge',.004)
for x in [-.816,.816]:box('Side timber lipping',(x,0,.736),(.012,.757,.038),'RO_WoodEdge',.003)
# Two welded steel supports; no centre pedestal fills the operator's legs.
for x in [-.748,.748]:
 for y in [-.307,.307]:
  box('Square tubular desk leg',(x,y,.366),(.037,.046,.712),'RO_Enamel',.004)
  box('Rubber levelling foot',(x,y,.007),(.044,.053,.014),'RO_DarkRubber',.003)
 box('Support upper rail',(x,0,.698),(.037,.638,.044),'RO_Enamel',.003)
 box('Low side brace',(x,.03,.177),(.032,.57,.026),'RO_Enamel',.003)
 for y in [-.305,.305]:cyl('Frame bolt',(x-.021,y,.687),.005,.005,'RO_WornMetal',(1,0,0),12)
box('Rear structural apron',(0,.324,.676),(1.475,.034,.080),'RO_Enamel',.004)
box('Discrete rear modesty panel',(-.215,.281,.450),(1.025,.026,.405),'RO_DrawerFace',.005)
box('Modesty lower return',(-.215,.282,.248),(1.030,.038,.018),'RO_Enamel',.003)
# Drawer pedestal is suspended from the right frame; toe clearance below remains.
box('Suspended drawer case',(.539,-.005,.425),(.365,.605,.535),'RO_Enamel',.005)
for centre,height in [(.605,.125),(.459,.145),(.265,.208)]:
 box('Drawer recessed reveal',(.539,-.311,centre),(.342,.010,height+.014),'RO_BezelDark',.002)
 box('Drawer front',(.539,-.323,centre),(.329,.021,height),'RO_DrawerFace',.004)
 for x in [.487,.591]:rod('Drawer pull foot',(x,-.335,centre+.014),(x,-.350,centre+.014),.0045,'RO_WornMetal')
 rod('Drawer pull',(.487,-.350,centre+.014),(.591,-.350,centre+.014),.005,'RO_WornMetal')
box('Small blank index',(.54,-.335,.637),(.075,.004,.024),'RO_LabelBlank',.001)
cyl('Drawer lock',(.655,-.336,.607),.008,.004,'RO_WornMetal',(0,-1,0),16)
# Rear cable tray, edge route and top grommet. No live socket/electrical interaction.
box('Cable tray floor',(-.14,.350,.602),(1.00,.075,.012),'RO_Enamel',.003)
box('Cable tray rear flange',(-.14,.383,.624),(1.00,.012,.055),'RO_Enamel',.003)
for x in [-.58,.28]:box('Tray hanger',(x,.347,.650),(.021,.032,.088),'RO_Enamel',.003)
cyl('Desk cable grommet',(.67,.265,.761),.024,.004,'RO_Bakelite',N=32)
box('Grommet slit',(.67,.265,.764),(.029,.007,.002),'RO_DarkRubber',.001)
finish_group('DeskStructure')
tube('Rear equipment cable',[(.67,.265,.765),(.691,.282,.766),(.720,.343,.768),(.735,.399,.746),(.728,.417,.686),(.65,.368,.620),(.32,.349,.616),(-.05,.356,.616),(-.42,.348,.616)],.003,'RO_DarkRubber')
for x in [-.36,.07,.30]:box('Cable tie',(x,.351,.619),(.008,.012,.011),'RO_Bakelite',.001)
finish_group('DeskRearCable')
export_asset('ReviewDesk',{'FLOOR_CONTACT':(0,0,0),'DESKTOP_CENTRE':(0,0,.760),'REAR_EDGE':(0,.390,.760),'KNEE_FRONT_LEFT':(-.710,-.390,.700),'KNEE_REAR_RIGHT':(.345,.248,.700),'MONITOR_PLACEMENT':(-.12,.12,.760),'TASKLAMP_PLACEMENT':(-.59,.03,.760)},'Floor centre root. Top 1.65x.78m, .760m high (grommet/cable rise to .769). Knee bay X approximately -.710..+.345; rear panel front Y.268. Two side frames plus a suspended right drawer pedestal. Static drawers, no interaction. DeskRearCable separate. No assets imported from outside project.')

# Compact articulated task light. Its origin is the table contact, not cable lowpoint.
for a in [0,2*pi/3,4*pi/3]:cyl('Task light rubber pad',(.057*cos(a),.057*sin(a),.0025),.009,.005,'RO_DarkRubber',N=16)
lathe('Weighted task base',[(.012,.005),(.074,.006),(.079,.015),(.075,.025),(.043,.036),(.024,.040)],'RO_Enamel',40)
cyl('Base swivel',(0,0,.049),.019,.034,'RO_WornMetal',N=24)
A=Vector((0,0,.065));B=Vector((0,.064,.265));C=Vector((0,-.115,.435))
for side in [-1,1]:
 delta=Vector((side*.012,0,0));rod('Lower articulated arm',A+delta,B+delta,.0055,'RO_Enamel');rod('Upper articulated arm',B+delta,C+delta,.0055,'RO_Enamel')
for i,p in enumerate([A,B,C]):
 cyl('Articulated hinge',p,.015 if i<2 else .012,.038,'RO_WornMetal',(1,0,0),24)
 cyl('Bakelite hinge knob',p+Vector((.027,0,0)),.012,.011,'RO_Bakelite',(1,0,0),20)
rod('Tension rod',A+Vector((0,.012,.017)),B+Vector((0,-.007,-.041)),.0023,'RO_WornMetal')
# Ribbed sleeve suggests the compact arm's spring without high-poly coil geometry.
d=B-A;mid=A+d*.47;cyl('Spring shroud',mid,.008,.086,'RO_Bakelite',d,24)
head=Vector((0,-.157,.415));angle=math.radians(-24);q=Vector((0,0,1)).rotation_difference(Vector((0,math.sin(-angle),math.cos(angle))))
# Rotation -24 degrees around X makes the open underside point forwards/down.
from mathutils import Matrix
rot=Matrix.Rotation(angle,4,'X')
def hp(p):return head+rot@Vector(p)
rod('Head pivot neck',C,hp((0,0,.032)),.007,'RO_WornMetal')
N=48;profile=[(.078,-.040),(.081,-.034),(.079,-.026),(.055,.025),(.026,.048),(.013,.050)];v=[tuple(hp((r*cos(i*2*pi/N),r*sin(i*2*pi/N),z))) for r,z in profile for i in range(N)];f=[]
for j in range(len(profile)-1):
 for i in range(N):a=j*N+i;b=j*N+(i+1)%N;f.append((a,b,b+N,a+N))
f.append(tuple((len(profile)-1)*N+i for i in range(N)));mesh('Spun enamel task shade',v,f,'RO_Enamel')
tube('Shade rolled rim',[tuple(hp((.079*cos(i*2*pi/32),.079*sin(i*2*pi/32),-.035))) for i in range(32)],.0022,'RO_WornMetal',True)
finish_group('TaskLightBody')
# Disc is physically recessed in the opening, with separate luminous material.
v=[tuple(hp((0,0,-.030)))]+[tuple(hp((.075*cos(i*2*pi/N),.075*sin(i*2*pi/N),-.030))) for i in range(N)];f=[(0,1+(i+1)%N,1+i) for i in range(N)];mesh('Opal task diffuser',v,f,'RO_LampDiffuser',False);finish_group('TaskLightDiffuser')
tube('Arm flex cable',[(.023,0,.032),(.023,.006,.09),(.025,.066,.235),(.025,.068,.28),(.024,-.090,.424),(.015,-.136,.445)],.0021,'RO_DarkRubber')
cord=tube('Desk edge cord',[(0,.07,.020),(.04,.105,.006),(.08,.170,.0028),(.064,.255,.0028),(.10,.325,.0028),(.112,.360,.0028),(.12,.392,-.029),(.12,.420,-.095)],.0028,'RO_DarkRubber')
finish_group('TaskLightCable')
export_asset('TaskLamp',{'TABLE_CONTACT':(0,0,0),'LIGHT_SOURCE':tuple(hp((0,0,-.033))),'LIGHT_AIM_TIP':tuple(hp((0,0,-.233))),'DIFFUSER_CENTRE':tuple(hp((0,0,-.030))),'TABLE_BACK_EDGE':(0,.36,0),'CORD_END':(.12,.42,-.095)},'Table-contact origin. Cord bends over table back plane localY+.36; at the suggested desktop source Y+.03 it meets the .39m desk rear edge. End falls below table, so whole model minZ is intentionally negative. Static jointed lamp, body/diffuser/cable separate. No FBX light; emission initially off.')

# Review monitor: physical screen aperture at Y0, never a new screen surface.
# Screen rectangle .690x.380 remains entirely open from the front to that plane.
innerW=.706;innerH=.398;outerW=.858;outerH=.538
for x in [-1,1]:box('Bezel side',(x*(innerW/2+(outerW-innerW)/4),.025,0),((outerW-innerW)/2,.092,outerH),'RO_MonitorEnamel',.007)
for z in [-1,1]:box('Bezel horizontal',(0,.025,z*(innerH/2+(outerH-innerH)/4)),(innerW,.092,(outerH-innerH)/2),'RO_MonitorEnamel',.006)
# Thin dark rebate is outside the screen clear rectangle, rather than over it.
for x in [-1,1]:box('Dark inner side rebate',(x*.3515,.003,0),(.006,.030,.390),'RO_BezelDark',.0015)
for z in [-1,1]:box('Dark inner horizontal rebate',(0,.003,z*.1965),(.697,.030,.005),'RO_BezelDark',.001)
# Rear shoulder is a tapered closed chassis, entirely behind the screen plane.
profiles=[(.414,.251,.067),(.404,.244,.095),(.366,.220,.163),(.331,.202,.183)];v=[]
for w,h,y in profiles:
 cut=.016
 for x,z in [(-w+cut,-h),(w-cut,-h),(w,-h+cut),(w,h-cut),(w-cut,h),(-w+cut,h),(-w,h-cut),(-w,-h+cut)]:v.append((x,y,z))
f=[]
for j in range(len(profiles)-1):
 for i in range(8):a=j*8+i;b=j*8+(i+1)%8;f.append((a,a+8,b+8,b))
f.append(tuple((len(profiles)-1)*8+i for i in reversed(range(8))));mesh('Tapered monitor rear chassis',v,f,'RO_MonitorEnamel',False)
for x in [-.395,.395]:
 for z in [-.23,.23]:cyl('Recessed bezel screw',(x,-.023,z),.0035,.003,'RO_WornMetal',(0,-1,0),12)
for i in range(8):box('Rear cooling slot',(-.19+i*.054,.184,.133),(.037,.003,.006),'RO_BezelDark',.001)
box('Rear blank asset label',(0,.185,-.075),(.145,.003,.062),'RO_LabelBlank',.002)
box('Cable connector recess',(.22,.182,-.134),(.069,.012,.033),'RO_BezelDark',.003)
finish_group('MonitorShellOpen')
# Stand height solves screen centre1.480 to desktop.760 exactly.
box('Monitor base underside',(0,.083,-.715),(.405,.282,.010),'RO_DarkRubber',.004)
box('Cast monitor foot',(0,.083,-.696),(.400,.278,.032),'RO_Enamel',.010)
box('Stem foot socket',(0,.119,-.666),(.107,.083,.045),'RO_MonitorEnamel',.007)
box('Tapered visual stem core',(0,.129,-.478),(.063,.064,.370),'RO_MonitorEnamel',.007)
box('Stem rear reinforcement',(0,.166,-.476),(.033,.021,.346),'RO_Enamel',.004)
cyl('Monitor tilt hinge',(0,.131,-.264),.038,.162,'RO_WornMetal',(1,0,0),32)
box('Chassis stand saddle',(0,.139,-.205),(.136,.074,.106),'RO_Enamel',.006)
finish_group('MonitorStand')
tube('Monitor rear cable',[(.22,.192,-.135),(.229,.205,-.181),(.18,.204,-.279),(.073,.184,-.402),(.064,.181,-.548),(.089,.167,-.679),(.168,.171,-.706),(.220,.193,-.717)],.003,'RO_DarkRubber')
finish_group('MonitorCable')
export_asset('ReviewMonitor',{'SCREEN_CENTRE':(0,0,0),'SCREEN_TOP_RIGHT':(.345,0,.190),'SCREEN_BOTTOM_LEFT':(-.345,0,-.190),'DESKTOP_CONTACT':(0,0,-.720),'BASE_CENTRE':(0,.083,-.720),'REAR_CABLE_END':(.220,.193,-.717)},'Root at existing screen centre. Front -Y. Display .690x.380m sits in planeY0 inside true open aperture; no exported display/text plane. Bottom of stand Z-.720 meets desktop .760 when screen is worldY1.480. Orient corrected Unity nominalfront+Z toward world+X using yaw90. Source chassis behind+Y; adapt placement/stand only if screen height changes.')

triangles=sum(sum(m['triangles'] for m in a['mesh_objects']) for a in assets.values())
if triangles>20000:raise RuntimeError('RecoveryOffice exceeds requested 20k triangle limit: '+str(triangles))
manifest={'date':'2026-10-01','authorship':'Original project geometry and material values. No external models/images/textures/fonts/services. Procedural timber preview noise is not baked/exported. No new recorded data or gameplay.','axis_contract':'Source Xwidth,-Yfront,Zup. FBX -Z/+Y. Observed Unity importer maps(-X,Z,-Y): negate imported root localScale.x once, retain imported magnitude/rotation, then placementRotation*importedRotation. Corrected map(X,Z,-Y), nominalfrontUnity+Z. Use marker position differences for directions.','materials':VALUES,'total_triangles':triangles,'assets':assets,'lamp_emission':'RO_LampDiffuser starts off. Track actual Light enabled/intensity; use TaskLamp__LIGHT_SOURCE and LIGHT_AIM_TIP to place an actual downward spot. No light object is exported. Shade/body can cast shadows; diffuser should not block its source.'}
json.dump(manifest,open(os.path.join(BASE,'manifest.json'),'w'),indent=2)
for root in roots:
 for o in [root]+list(root.children_recursive):o.hide_set(False);o.hide_render=False
# Save clean authoring source with assets separated along X for editable inspection.
for i,root in enumerate(roots):root.location.x=i*2.8
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BASE,'EchoRecoveryOffice.blend'))
for root in roots:root.location=(0,0,0)

# Studio previews only. All geometry was exported before preview setup.
scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True;scene.render.resolution_x=1200;scene.render.resolution_y=1000;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX';scene.world.color=(.06,.06,.06)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.003));ground=bpy.context.object;m=bpy.data.materials.new('Preview floor');m.use_nodes=True;m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.085,.092,.078,1);m.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.9;ground.data.materials.append(m)
for n,p,power,size,col in [('Key',(-3,-4,5),620,4,(1,.88,.71)),('Fill',(4,-2,3),330,3,(.67,.79,1)),('Rim',(1,3,4),430,3,(1,.91,.76))]:
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.size=size;d.color=col;o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=p;o.rotation_euler=(Vector((0,0,.8))-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;scene.camera=cam
def show(ids):
 for i,root in enumerate(roots):
  for o in [root]+list(root.children_recursive):o.hide_set(i not in ids);o.hide_render=i not in ids
def render(n,p,target,lens=52):
 cam.location=p;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.lens=lens;scene.render.filepath=os.path.join(PRE,n+'.png');bpy.ops.render.render(write_still=True)
show([0]);render('ReviewDesk-front',(-2.00,-2.80,1.65),(0,0,.46),53);render('ReviewDesk-rear',(1.90,2.65,1.38),(0,0,.47),53)
show([1]);roots[1].location.z=.12
bpy.ops.mesh.primitive_cube_add(size=1,location=(0,-.03,.105));sample_table=bpy.context.object;sample_table.name='Preview tabletop only';sample_table.dimensions=(1,.78,.03);sample_table.data.materials.append(mats['RO_Wood'])
render('TaskLamp',(-.63,-.83,.73),(0,.07,.31),52);sample_table.hide_render=True;roots[1].location.z=0
show([2]);roots[2].location.z=.72;render('Monitor-open',(-1.12,-1.63,1.00),(0,.05,.46),53);render('Monitor-rear',(1.0,1.72,1.0),(0,.08,.45),53);roots[2].location.z=0
show([0,1,2]);roots[1].location=(-.59,.03,.76);roots[2].location=(-.12,.12,1.48)
render('Assembled-workstation',(-2.1,-3.05,1.64),(0,0,.78),51)
# Exact display-size preview is never exported. It proves spatial fit, not text
# readability or interaction; no invented monitor function or content is supplied.
bpy.ops.mesh.primitive_cube_add(size=1);display=bpy.context.object;display.name='Preview existing display size only';display.dimensions=(.69,.001,.38);display.parent=roots[2];display.location=(0,0,0)
m=bpy.data.materials.new('Preview neutral screen');m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(.013,.050,.046,1);p.inputs['Roughness'].default_value=.6;p.inputs['Emission Color'].default_value=(.015,.058,.051,1);p.inputs['Emission Strength'].default_value=.35;display.data.materials.append(m)
render('Assembled-with-reference-display',(-2.1,-3.05,1.64),(0,0,.78),51)
show([2]);roots[2].location=(0,0,.72);render('Monitor-display-fit',(-1.12,-1.63,1.00),(0,.05,.46),53)
print('RECOVERY OFFICE COMPLETE',triangles,flush=True)
