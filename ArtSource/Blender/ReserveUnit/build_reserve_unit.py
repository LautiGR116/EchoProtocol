"""Original domestic reserve unit and wall dock. Metres; X width/-Y front/Z up.
Two original assets, no imported media or gameplay. Reuses original
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
 'RU_Enamel':{'color':(.14,.18,.145,1),'roughness':.54,'metallic':.25},
 'RU_Edge':{'color':(.075,.095,.070,1),'roughness':.50,'metallic':.34},
 'RU_Bakelite':{'color':(.027,.025,.019,1),'roughness':.42,'metallic':0},
 'RU_Rubber':{'color':(.013,.016,.014,1),'roughness':.80,'metallic':0},
 'RU_Nickel':{'color':(.27,.28,.23,1),'roughness':.41,'metallic':.76},
 'RU_Porcelain':{'color':(.59,.55,.42,1),'roughness':.41,'metallic':0},
 'RU_Label':{'color':(.46,.43,.32,1),'roughness':.80,'metallic':0},
 'RU_Ink':{'color':(.045,.055,.039,1),'roughness':.76,'metallic':0},
 'RU_Lens':{'color':(.32,.13,.018,1),'roughness':.32,'metallic':0}}
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
 root=bpy.data.objects.new('EP_RU_'+name,None);bpy.context.collection.objects.link(root);roots.append(root)
 for o in groups:o.parent=root
 anchors={'ROOT':(0,0,0),'AXIS_X':(.1,0,0),'AXIS_Y':(0,0.1,0),'AXIS_Z':(0,0,.1),'FRONT_BASE':(0,0,0),'FRONT_TIP':(0,-.1,0)};anchors.update(markers);full={}
 for n,p in anchors.items():
  fn=name+'__'+n;o=bpy.data.objects.new(fn,None);bpy.context.collection.objects.link(o);o.parent=root;o.location=p;o.empty_display_type='PLAIN_AXES';o.empty_display_size=.025;full[fn]=list(p)
 bpy.ops.object.select_all(action='DESELECT')
 for o in [root]+list(root.children_recursive):o.select_set(True)
 bpy.context.view_layer.objects.active=root;relative='exports/EP_RU_'+name+'.fbx';bpy.ops.export_scene.fbx(filepath=os.path.join(BASE,relative),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH','EMPTY'},bake_anim=False)
 coords=[o.matrix_world@v.co for o in groups for v in o.data.vertices];bb=[[min(p[i] for p in coords),max(p[i] for p in coords)] for i in range(3)]
 assets[name]={'file':relative,'bounds_blender_xyz':bb,'dimensions_width_depth_height':[b-a for a,b in bb],'materials':sorted({m.name for o in groups for m in o.data.materials}),'mesh_objects':[{'name':o.name,'vertices':len(o.data.vertices),'triangles':sum(len(f.vertices)-2 for f in o.data.polygons),'uv_layers':[u.name for u in o.data.uv_layers]} for o in groups],'anchors_blender_xyz':full,'notes':notes}
 for o in [root]+list(root.children_recursive):o.hide_set(True);o.hide_render=True
 groups=[];print('EXPORTED',name,assets[name]['dimensions_width_depth_height'],flush=True)

# Pack root at its bottom centre. Shrouded terminals face the rear (+Y).
# Compact metal shell, not a fuse: the portable reserve has actual volume/weight.
box('Rubber lower gasket',(0,0,.008),(.136,.073,.016),'RU_Rubber',.007)
box('Deep drawn accumulator case',(0,0,.082),(.134,.072,.140),'RU_Enamel',.010)
box('Formed top cap',(0,0,.155),(.137,.075,.014),'RU_Edge',.006)
for x in [-.057,.057]:
 box('Vertical edge rib',(x,-.037,.084),(.008,.005,.119),'RU_Edge',.002)
 for z in [.025,.141]:
  cyl('Case screw',(x,-.041,z),.003,.003,'RU_Nickel',(0,-1,0),12)
  box('Screw slot',(x,-.043,z),(.004,.001,.0008),'RU_Ink',.0002)
# Folded loop grip: its clear opening reads from the dock and the carried view.
for x in [-.046,.046]:
 box('Handle ear',(x,0,.170),(.018,.025,.031),'RU_Bakelite',.006)
 cyl('Handle pivot',(x,0,.169),.006,.029,'RU_Nickel',(0,1,0),16)
rod('Pull grip core',(-.047,0,.201),(.047,0,.201),.010,'RU_Bakelite')
for x in [-.046,.046]:rod('Handle rising limb',(x,0,.168),(x,0,.201),.007,'RU_Bakelite')
rod('Ivory grip inset',(-.033,-.001,.204),(.033,-.001,.204),.007,'RU_Porcelain')
# Front designation plate, small readiness lens and a ridge identifying orientation.
box('Recessed identity plate',(0,-.039,.108),(.080,.003,.042),'RU_Label',.003)
box('Readiness bezel',(0,-.040,.063),(.042,.005,.018),'RU_Bakelite',.002)
box('Readiness amber glass',(0,-.0435,.063),(.029,.002,.008),'RU_Lens',.001)
# Direction arrow, simple original geometry, no imported font.
mesh('Raised insertion arrow',[(-.012,-.040,.035),(-.004,-.040,.035),(-.004,-.040,.047),(.004,-.040,.047),(.004,-.040,.035),(.012,-.040,.035),(0,-.040,.023)],[(0,1,2,3,4,5,6)],'RU_Porcelain',False)
# Closed shrouds keep contacts recessed, not two exposed brass spikes.
for x in [-.025,.025]:
 box('Rear connector shroud',(x,.037,.102),(.033,.012,.046),'RU_Bakelite',.004)
 box('Recessed terminal socket',(x,.044,.104),(.012,.001,.024),'RU_Rubber',.002)
 box('Terminal inner contact',(x,.0448,.104),(.006,.001,.014),'RU_Nickel',.001)
box('Rear locating notch',(0,.037,.027),(.025,.006,.014),'RU_Rubber',.002)
finish_group('ReservePackBody')
export_asset('ReservePack',{'BOTTOM':(0,0,0),'GRIP':(0,0,.201),'LABEL_CENTRE':(0,-.041,.108),'READINESS_LENS':(0,-.045,.063),'REAR_CONNECTOR':(0,.044,.104)},'Compact reserve accumulator with protective connector shrouds and carrying grip. Bottom root, source front-Y. .137m wide x .089m deep x .211m high. Insert into ReserveDock using DOCKED_PACK_BOTTOM; portable representation uses the same model. No actual light/voice or charge/drain mechanic.')
# Mural dock: root at wall-plane centre, source front -Y, extends entirely into room.
box('Wall insulation gasket',(0,-.003,0),(.206,.006,.282),'RU_Rubber',.008)
box('Baked enamel mounting plate',(0,-.010,0),(.208,.010,.284),'RU_Porcelain',.009)
for x in [-.088,.088]:
 for z in [-.119,.119]:
  cyl('Wall mounting screw',(x,-.017,z),.004,.003,'RU_Nickel',(0,-1,0),16)
  box('Wall screw slot',(x,-.019,z),(.005,.001,.001),'RU_Ink',.0002)
box('Shielded dock recess',(0,-.019,-.016),(.149,.010,.177),'RU_Bakelite',.003)
# Rails form a true empty opening when the reserve is removed.
for x in [-.077,.077]:
 box('Raised receiver side rail',(x,-.033,-.020),(.010,.025,.172),'RU_Edge',.003)
 box('Rail return flange',(x*1.045,-.047,-.020),(.015,.007,.165),'RU_Nickel',.002)
box('Dock bottom ledge',(0,-.045,-.110),(.165,.065,.010),'RU_Edge',.004)
box('Bottom cushioning',(0,-.057,-.103),(.132,.044,.005),'RU_Rubber',.001)
# Recessed power/data pads hidden behind mounted unit. Keep a legible empty silhouette.
for x in [-.025,.025]:
 box('Dock contact housing',(x,-.026,-.001),(.034,.012,.046),'RU_Rubber',.003)
 for z in [-.01,.01]:box('Dock recessed pad',(x,-.033,z),(.010,.001,.007),'RU_Nickel',.001)
box('Dock small ceramic index',(0,-.017,.119),(.132,.003,.025),'RU_Label',.002)
# Engraved-like empty socket witness bars and unobtrusive wire entry at underside.
for z in [-.07,-.044,.044,.068]:box('Inner socket guide',(0,-.025,z),(.045,.001,.002),'RU_Porcelain',.0004)
cyl('Insulated bottom cable entry',(0,-.009,-.148),.008,.028,'RU_Bakelite',N=20)
finish_group('ReserveDockBody')
export_asset('ReserveDock',{'WALL_PLANE':(0,0,0),'DOCKED_PACK_BOTTOM':(0,-.081,-.100),'LABEL_CENTRE':(0,-.020,.119),'EMPTY_SOCKET_CENTRE':(0,-.035,-.016)},'Root at wall plane. All geometry extends toward source-Y/front, apart from flush wall gasket. Protected recess and guide rails; pack bottom at(0,-.081,-.100). Label blank intended for actual room name. No collider or active logic exported.')
triangles=sum(m['triangles'] for a in assets.values() for m in a['mesh_objects'])
manifest={'date':'2026-10-01','authorship':'Original project geometry/materials; no third-party model, texture, font or service.','axis_contract':'Source Xwidth,-Yfront,Zup. FBX -Z/+Y; Unity negate imported root X once, retain import scale/rotation, then placement rotation. Corrected(X,Z,-Y).','materials':VALUES,'total_triangles':triangles,'assets':assets}
json.dump(manifest,open(os.path.join(BASE,'manifest.json'),'w'),indent=2)
for i,r in enumerate(roots):
 for o in [r]+list(r.children_recursive):o.hide_set(False);o.hide_render=False
 r.location.x=i*.40
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BASE,'EchoReserveUnit.blend'))
for r in roots:r.location=(0,0,0)
scene.render.engine='CYCLES';scene.cycles.samples=32;scene.cycles.use_denoising=True;scene.render.resolution_x=1200;scene.render.resolution_y=1000;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX';scene.world.color=(.06,.06,.06)
for n,p,power,size,col in [('Key',(-.6,-.8,1),55,1,(1,.90,.77)),('Fill',(.6,-.4,.6),28,1,(.65,.78,1)),('Rim',(0,.7,.8),35,.8,(1,.95,.8))]:
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.size=size;d.color=col;o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=p;o.rotation_euler=(Vector((0,0,.1))-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;scene.camera=cam;cam.data.lens=58
roots[1].location.z=.14
# Simple wall exists only in studio previews, not in exported source assets.
bpy.ops.mesh.primitive_cube_add(size=1,location=(0,.02,.14));wall=bpy.context.object;wall.dimensions=(1.4,.03,1);wall.data.materials.append(mats['RU_Edge'])
for name,occupied in [('Reserve-docked',True),('Reserve-empty',False)]:
 roots[0].location=(0,-.081,.04)
 for o in [roots[0]]+list(roots[0].children_recursive):o.hide_render=not occupied
 cam.location=(-.43,-.67,.40);cam.rotation_euler=(Vector((0,-.045,.16))-cam.location).to_track_quat('-Z','Y').to_euler();scene.render.filepath=os.path.join(PRE,name+'.png');bpy.ops.render.render(write_still=True)
print('RESERVE UNIT COMPLETE',triangles,flush=True)
