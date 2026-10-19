"""Original 1.61 m domestic floor lamp. Blender 5.2 LTS, metres.
Standalone source, no external media/addons. X width/-Y front/Z up.
FBX root X needs the same Unity importer correction as PressurePlant.
"""
import bpy, os, json, math
from math import sin, cos, pi
from mathutils import Vector
BASE=os.path.dirname(os.path.abspath(__file__));OUT=os.path.join(BASE,'exports');PRE=os.path.join(BASE,'previews')
for p in [OUT,PRE]:os.makedirs(p,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
VALUES={
 'DL_BaseEnamel':{'color':(.052,.067,.052,1),'roughness':.49,'metallic':.36},
 'DL_Brass':{'color':(.27,.188,.088,1),'roughness':.42,'metallic':.75},
 'DL_Walnut':{'color':(.10,.052,.024,1),'roughness':.50,'metallic':0},
 'DL_ShadeLinen':{'color':(.49,.425,.306,1),'roughness':.88,'metallic':0},
 'DL_ShadeBinding':{'color':(.285,.248,.18,1),'roughness':.84,'metallic':0},
 'DL_InnerDiffuser':{'color':(.69,.64,.50,1),'roughness':.83,'metallic':0},
 'DL_OpalBulb':{'color':(.79,.735,.58,1),'roughness':.36,'metallic':0},
 'DL_Rubber':{'color':(.012,.016,.012,1),'roughness':.77,'metallic':0},
 'DL_Bakelite':{'color':(.047,.037,.022,1),'roughness':.39,'metallic':0},
 'DL_SocketIvory':{'color':(.38,.385,.295,1),'roughness':.49,'metallic':0}}
mats={};parts=[];groups=[]
for n,d in VALUES.items():
 m=bpy.data.materials.new(n);m.use_nodes=True;m.diffuse_color=d['color'];p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=d['color'];p.inputs['Roughness'].default_value=d['roughness'];p.inputs['Metallic'].default_value=d['metallic'];mats[n]=m
 if n=='DL_ShadeLinen':
  t=m.node_tree.nodes.new('ShaderNodeTexNoise');t.inputs['Scale'].default_value=400;t.inputs['Detail'].default_value=1
  b=m.node_tree.nodes.new('ShaderNodeBump');b.inputs['Strength'].default_value=.09;b.inputs['Distance'].default_value=.001;m.node_tree.links.new(t.outputs['Fac'],b.inputs['Height']);m.node_tree.links.new(b.outputs['Normal'],p.inputs['Normal'])
def add(o,mat):o.data.materials.append(mats[mat]);parts.append(o);return o
def mesh(n,v,f,mat,smooth=True):
 me=bpy.data.meshes.new(n);me.from_pydata(v,[],f);me.update();o=bpy.data.objects.new(n,me);bpy.context.collection.objects.link(o);add(o,mat)
 for p in me.polygons:p.use_smooth=smooth
 return o
def lathe(n,profile,mat,N=64):
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
  m=o.modifiers.new('Rounded formed edge','BEVEL');m.width=b;m.segments=3;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=m.name)
  for f in o.data.polygons:f.use_smooth=True
  m=o.modifiers.new('Weighted normals','WEIGHTED_NORMAL');m.keep_sharp=True;bpy.ops.object.modifier_apply(modifier=m.name)
 return o
def tube(n,pts,r,mat,closed=False):
 cu=bpy.data.curves.new(n,'CURVE');cu.dimensions='3D';cu.resolution_u=1 if len(pts)>16 else 8;cu.bevel_depth=r;cu.bevel_resolution=1;sp=cu.splines.new('BEZIER');sp.bezier_points.add(len(pts)-1)
 for j,(b,p) in enumerate(zip(sp.bezier_points,pts)):
  p=Vector(p);prev=Vector(pts[(j-1)%len(pts)] if closed or j>0 else pts[j]);nxt=Vector(pts[(j+1)%len(pts)] if closed or j<len(pts)-1 else pts[j]);t=(nxt-prev)/6;left=p-t;right=p+t
  for k in range(3):left[k]=max(min(prev[k],p[k]),min(max(prev[k],p[k]),left[k]));right[k]=max(min(nxt[k],p[k]),min(max(nxt[k],p[k]),right[k]))
  b.co=p;b.handle_left_type='FREE';b.handle_right_type='FREE';b.handle_left=left;b.handle_right=right
 sp.use_cyclic_u=closed;o=bpy.data.objects.new(n,cu);bpy.context.collection.objects.link(o);bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o;bpy.ops.object.convert(target='MESH');o=bpy.context.object;add(o,mat)
 for f in o.data.polygons:f.use_smooth=True
 return o
def rod(n,a,b,r,mat):d=Vector(b)-Vector(a);return cyl(n,(Vector(a)+Vector(b))/2,r,d.length,mat,d)
def ring(n,r,z,thick,mat):return tube(n,[(r*cos(a),r*sin(a),z) for a in [i*2*pi/64 for i in range(64)]],thick,mat,True)
def finish_group(n):
 global parts
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name=n;scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(angle_limit=1.1519,island_margin=.012,scale_to_bounds=True);bpy.ops.object.mode_set(mode='OBJECT');o.data.uv_layers.active.name='UVMap';groups.append(o);parts=[];return o

# Heavy low spun base, rubber feet, turned timber stem and worn brass ferrules.
for a in [0,2*pi/3,4*pi/3]:cyl('Floor pad',(.126*cos(a),.126*sin(a),.004),.019,.008,'DL_Rubber',N=20)
lathe('Weighted enamel base',[(.018,.006),(.148,.009),(.164,.018),(.165,.030),(.157,.043),(.118,.058),(.046,.071),(.029,.091)],'DL_BaseEnamel')
ring('Base rolled lip',.162,.025,.0025,'DL_Brass')
lathe('Lower brass ferrule',[(.028,.072),(.031,.08),(.028,.113),(.023,.126)],'DL_Brass',40)
lathe('Turned walnut stem',[(.023,.108),(.025,.19),(.018,.49),(.015,.90),(.017,1.235),(.020,1.272)],'DL_Walnut',40)
lathe('Upper ferrule',[(.020,1.24),(.022,1.263),(.022,1.293),(.018,1.307)],'DL_Brass',40)
cyl('Socket neck',(0,0,1.337),.018,.077,'DL_Brass')
cyl('Bakelite bulb socket',(0,0,1.373),.021,.044,'DL_Bakelite',N=32)
for z in [1.357,1.367]:ring('Socket grip ridge',.021,z,.0014,'DL_Bakelite')
rod('Rotary switch shaft',(.018,0,1.373),(.042,0,1.373),.003,'DL_Brass');cyl('Switch knob',(.047,0,1.373),.008,.012,'DL_Bakelite',(1,0,0),20)
# Shade harp and radial spider are thin support hardware, not silhouette clutter.
for side in [-1,1]:tube('Shade harp',[(side*.015,0,1.332),(side*.061,0,1.35),(side*.062,0,1.535),(side*.018,0,1.583),(0,0,1.586)],.0023,'DL_Brass')
for a in [pi/6,pi*5/6,pi*3/2]:rod('Shade spider',(0,0,1.587),(.159*cos(a),.159*sin(a),1.595),.0017,'DL_Brass')
cyl('Shade retaining nut',(0,0,1.599),.008,.010,'DL_Brass',N=20);sphere('Small shade finial',(0,0,1.608),(.010,.010,.008),'DL_Bakelite')
# Strain relief is at rear of base; cord may be hidden independently if rerouted.
cyl('Cable grommet',(0,.151,.037),.008,.016,'DL_Rubber',(0,1,0),20)
finish_group('LampBody')

# Hollow, subtly irregular linen shade with real inside surface and edge thickness.
N=96;z0=1.272;z1=1.598;r0=.232;r1=.160
def shade_point(i,t,inner=False):
 a=i*2*pi/N;r=r0+(r1-r0)*t
 ripple=.0010*sin(14*a+.2)*sin(pi*t)+.00065*sin(3*a+1.2)*sin(pi*t)
 r+=ripple-(.0028 if inner else 0)
 return (r*cos(a),r*sin(a),z0+(z1-z0)*t+.0007*sin(2*a)*sin(pi*t))
for inside,name,mat in [(False,'ShadeOuter','DL_ShadeLinen'),(True,'ShadeInner','DL_InnerDiffuser')]:
 steps=6;v=[shade_point(i,j/steps,inside) for j in range(steps+1) for i in range(N)];f=[]
 for j in range(steps):
  for i in range(N):a=j*N+i;b=j*N+(i+1)%N;quad=(a,b,b+N,a+N);f.append(tuple(reversed(quad)) if inside else quad)
 mesh(name+' fabric',v,f,mat)
 if not inside:
  ring('Lower stitched shade binding',r0-.001,z0,.0034,'DL_ShadeBinding');ring('Upper stitched shade binding',r1-.001,z1,.0030,'DL_ShadeBinding')
  tube('Shade rear seam',[shade_point(24,j/12) for j in range(13)],.0011,'DL_ShadeBinding')
 finish_group(name)
lathe('Bulb neck',[(.013,1.383),(.015,1.412),(.023,1.427)],'DL_OpalBulb',32)
sphere('Opal incandescent bulb',(0,0,1.452),(.033,.033,.047),'DL_OpalBulb');finish_group('OpalBulb')

# One original cord route to a rear wall 0.90 m from lamp centre. Separate meshes.
# Long floor portions have cable centre at .0032 m, so the rubber actually rests.
cord=tube('Rubber mains cord',[(0,.158,.037),(.008,.204,.011),(.060,.29,.0032),(.088,.405,.0032),(.048,.543,.0032),(-.043,.638,.0032),(-.067,.750,.0032),(-.038,.813,.038),(-.018,.842,.160),(0,.845,.247)],.0032,'DL_Rubber')
# The curve's polygonal cross-section is slightly inset from nominal radius.
# Set its actual lowest vertex onto the floor, instead of accepting a sub-mm gap.
cord.location.z-=min(v.co.z for v in cord.data.vertices)
box('Bakelite plug',(0,.855,.262),(.042,.055,.052),'DL_Bakelite',.008)
cyl('Plug cable relief',(0,.845,.234),.007,.025,'DL_Rubber')
finish_group('CordToRearWall')
box('Wall socket back',(0,.893,.272),(.080,.014,.105),'DL_Bakelite',.006)
box('Ivory socket plate',(0,.880,.272),(.078,.014,.102),'DL_SocketIvory',.006)
for z in [.235,.309]:
 cyl('Socket screw',(0,.871,z),.003,.002,'DL_Brass',(0,-1,0),12)
box('Socket contact recess',(0,.869,.272),(.027,.004,.032),'DL_Bakelite',.005)
finish_group('WallSocket')

root=bpy.data.objects.new('EP_DL_FloorLamp',None);bpy.context.collection.objects.link(root)
for o in groups:o.parent=root
markers={'AXIS_X':(.1,0,0),'AXIS_Y':(0,.1,0),'AXIS_Z':(0,0,.1),'FRONT_BASE':(0,0,0),'FRONT_TIP':(0,-.1,0),'FLOOR_CONTACT':(0,0,0),'BULB_SOURCE':(0,0,1.451),'SHADE_CENTRE':(0,0,1.435),'LIGHT_DOWN_TIP':(0,0,1.251),'CORD_EXIT':(0,.158,.037),'WALL_CONTACT':(0,.900,.272),'WALL_NORMAL_TIP':(0,.800,.272)}
anchors={}
for n,p in markers.items():
 full='FloorLamp__'+n;o=bpy.data.objects.new(full,None);bpy.context.collection.objects.link(o);o.parent=root;o.location=p;o.empty_display_type='PLAIN_AXES';o.empty_display_size=.035;anchors[full]=list(p)
bpy.ops.object.select_all(action='DESELECT')
for o in [root]+list(root.children_recursive):o.select_set(True)
bpy.context.view_layer.objects.active=root;relative='exports/EP_DL_FloorLamp.fbx';bpy.ops.export_scene.fbx(filepath=os.path.join(BASE,relative),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH','EMPTY'},bake_anim=False)
def bounds(items):
 pts=[o.matrix_world@v.co for o in items for v in o.data.vertices];return [[min(p[i] for p in pts),max(p[i] for p in pts)] for i in range(3)]
bb=bounds(groups);core=bounds(groups[:4])
asset={'file':relative,'bounds_blender_xyz':bb,'lamp_only_bounds_blender_xyz':core,'dimensions_width_depth_height':[b-a for a,b in bb],'lamp_only_dimensions_width_depth_height':[b-a for a,b in core],'materials':sorted({m.name for o in groups for m in o.data.materials}),'mesh_objects':[{'name':o.name,'vertices':len(o.data.vertices),'triangles':sum(len(f.vertices)-2 for f in o.data.polygons),'uv_layers':[u.name for u in o.data.uv_layers]} for o in groups],'anchors_blender_xyz':anchors}
doc={'date':'2026-10-01','authorship':'Original project model and source. No external models/images/fonts/textures/services. Procedural linen preview bump is not baked or exported.','axis_contract':'Source X width,-Y front,Z up. Current Unity importer maps (-X,Z,-Y). Negate imported root localScale.x once; preserve imported magnitudes/rotation; then placementRotation * importedRotation. Corrected map (X,Z,-Y). Direction comes from marker position differences.','materials':VALUES,'emission_contract':{'exported_emission':'off; no FBX light objects','suggested_light_anchor':'FloorLamp__BULB_SOURCE','separate_surfaces':['ShadeOuter','ShadeInner','OpalBulb'],'note':'DL_InnerDiffuser and DL_OpalBulb may track actual Light enabled/intensity. DL_ShadeLinen may use restrained linked emission to suggest transmitted light. Never leave any surface lit when practical is off. Shader/strength/shadow response must be set and viewed in Unity. Opaque shade can block a point light: use excluded shade shadow casting or authored aperture pools while body retains contact shadows.'},'assets':{'FloorLamp':asset}}
json.dump(doc,open(os.path.join(BASE,'manifest.json'),'w'),indent=2)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BASE,'EchoDomesticLighting.blend'))

# Studio inspection only, never exported. Wall establishes the optional socket contact.
scene.render.engine='CYCLES';scene.cycles.samples=32;scene.cycles.use_denoising=True;scene.render.resolution_x=1000;scene.render.resolution_y=1100;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX';scene.world.color=(.035,.035,.035)
for n,p,s,col in [('Preview floor',(0,0,-.035),(35,35,.07),(.105,.083,.057,1)),('Preview wall',(0,.94,1.40),(16,.08,2.8),(.13,.154,.122,1))]:
 m=bpy.data.materials.new(n);m.diffuse_color=col;m.use_nodes=True;m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=col;m.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.86;bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=n;o.dimensions=s;o.data.materials.append(m)
studio=[]
for n,p,power,size,col in [('Key',(-3,-4,4),500,3,(1,.9,.77)),('Fill',(3,-2,3),180,3,(.70,.80,1))]:
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.size=size;d.color=col;o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=p;o.rotation_euler=(Vector((0,0,.9))-o.location).to_track_quat('-Z','Y').to_euler();studio.append(o)
d=bpy.data.lights.new('Preview aperture practical','POINT');d.energy=0;d.color=(1,.78,.51);d.shadow_soft_size=.065;light=bpy.data.objects.new('Preview aperture practical',d);bpy.context.collection.objects.link(light);light.location=(0,0,1.27)
bpy.ops.object.camera_add();cam=bpy.context.object;scene.camera=cam
def lit(on):
 for name,strength in [('DL_ShadeLinen',.65),('DL_InnerDiffuser',1.3),('DL_OpalBulb',3.0)]:
  p=mats[name].node_tree.nodes.get('Principled BSDF');p.inputs['Emission Color'].default_value=(1,.79,.50,1);p.inputs['Emission Strength'].default_value=strength if on else 0
 d.energy=26 if on else 0
 for o in studio:o.data.energy*=.16 if on else 1
def render(n,p,target,lens=52):
 cam.location=p;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.lens=lens;scene.render.filepath=os.path.join(PRE,n+'.png');bpy.ops.render.render(write_still=True)
render('FloorLamp-off',(-1.8,-2.65,1.63),(0,.18,.86),52)
render('Shade-construction',(.56,-.82,1.70),(0,0,1.46),58)
render('Cable-wall-contact',(-.62,-.28,.55),(0,.49,.18),52)
lit(True);render('FloorLamp-on',(-1.8,-2.65,1.63),(0,.18,.86),52)
scene.render.resolution_x=1280;scene.render.resolution_y=720;scene.cycles.samples=24
lens=36/(2*(1280/720)*math.tan(pi/6))
render('At-8m-vertical-FOV60',(0,-8,1.63),(0,0,1.63),lens)
render('At-13m-vertical-FOV60',(0,-13,1.63),(0,0,1.63),lens)
print('DOMESTIC LIGHTING COMPLETE',asset['lamp_only_dimensions_width_depth_height'],sum(x['triangles'] for x in asset['mesh_objects']),flush=True)
