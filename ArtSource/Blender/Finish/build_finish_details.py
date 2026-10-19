"""Original Echo Protocol finish details. Blender 5.x, metres, project-authored geometry."""
import bpy, math, os
from mathutils import Vector
OUT=os.path.abspath('Assets/_Game/Art/Models/Finish')
os.makedirs(OUT,exist_ok=True)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
COLORS={'Steel':(.13,.19,.19,1),'Enamel':(.52,.58,.5,1),'Wood':(.18,.075,.029,1),'Fabric':(.24,.12,.07,1),'Ivory':(.73,.66,.47,1),'Black':(.018,.025,.023,1),'Glass':(.11,.27,.27,1),'Brass':(.45,.28,.085,1),'Echo':(.2,.6,.57,1)}
mats={}
for n,c in COLORS.items():
 m=bpy.data.materials.new(n);m.diffuse_color=c;m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=c;p.inputs['Roughness'].default_value=.62;p.inputs['Metallic'].default_value=.55 if n in ['Steel','Brass'] else 0;mats[n]=m
parts=[]
def cube(n,p,s,mat='Steel',bevel=.018,rot=(0,0,0)):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p,rotation=rot);o=bpy.context.object;o.name=n;o.dimensions=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(mats[mat]);parts.append(o)
 if bevel:
  mod=o.modifiers.new('Machined / worn edge','BEVEL');mod.width=bevel;mod.segments=3;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
 return o
def cyl(n,p,r,depth,mat='Steel',rot=(0,0,0),vertices=24):
 bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=depth,location=p,rotation=rot);o=bpy.context.object;o.name=n;o.data.materials.append(mats[mat]);parts.append(o)
 for poly in o.data.polygons:poly.use_smooth=True
 return o
def rod(n,a,b,r,mat='Steel'):
 d=Vector(b)-Vector(a);o=cyl(n,(Vector(a)+Vector(b))/2,r,d.length,mat);o.rotation_euler=d.to_track_quat('Z','Y').to_euler();return o
def sphere(n,p,s,mat='Echo'):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=20,ring_count=12,location=p);o=bpy.context.object;o.name=n;o.scale=s;o.data.materials.append(mats[mat]);parts.append(o)
 for poly in o.data.polygons:poly.use_smooth=True
 return o
def torus(n,p,r,t,mat='Steel',rot=(0,0,0)):
 bpy.ops.mesh.primitive_torus_add(major_radius=r,minor_radius=t,major_segments=32,minor_segments=10,location=p,rotation=rot);o=bpy.context.object;o.name=n;o.data.materials.append(mats[mat]);parts.append(o);return o
def save(name):
 global parts
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name='EP_'+name
 bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'EP_'+name+'.fbx'),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH'},bake_anim=False)
 o.hide_set(True);parts=[]

# Station bodies face -Y; Blender Z is vertical.
cube('Enamel backplate',(0,0,0),(.53,.085,.61),'Enamel',.025)
for x in [-.215,.215]:
 for z in [-.25,.25]:cyl('Screw',(x,-.05,z),.009,.012,'Steel',(math.pi/2,0,0),12)
rod('Shaft',(0,-.05,-.04),(0,-.21,-.04),.024)
torus('Handwheel',(0,-.225,-.04),.205,.023,'Brass',(math.pi/2,0,0))
for i in range(5):
 a=i*math.pi*2/5;rod('Spoke',(0,-.225,-.04),(.185*math.cos(a),-.225,-.04+.185*math.sin(a)),.012,'Brass')
cyl('Hub',(0,-.225,-.04),.04,.045,'Steel',(math.pi/2,0,0))
cyl('Gauge case',(0,-.10,.38),.14,.095,'Steel',(math.pi/2,0,0));cyl('Dial',(0,-.152,.38),.121,.008,'Ivory',(math.pi/2,0,0))
for i in range(11):
 a=math.radians(30+i*24);rod('Gauge tick',(.099*math.sin(a),-.16,.38+.099*math.cos(a)),(.111*math.sin(a),-.16,.38+.111*math.cos(a)),.002,'Black')
save('ValveControl')
cube('Fuse box',(0,0,0),(.36,.12,.5),'Enamel',.025)
cube('Recess',(0,-.068,.035),(.27,.01,.29),'Black',.008)
for x in [-.085,0,.085]:
 cyl('Ceramic fuse',(x,-.1,.04),.027,.045,'Ivory',(math.pi/2,0,0),16)
 cube('Contact',(x,-.13,.04),(.015,.016,.031),'Brass',.003)
cube('Rating plate',(0,-.067,-.19),(.21,.012,.06),'Ivory',.003)
for x in [-.145,.145]:
 for z in [-.212,.212]:cyl('Screw',(x,-.067,z),.008,.012,'Steel',(math.pi/2,0,0),12)
save('FuseBox')
cube('Board',(0,0,0),(.34,.018,.46),'Wood',.009);cube('Paper',(0,-.014,-.01),(.30,.006,.40),'Ivory',.002)
cube('Clip',(0,-.022,.21),(.12,.025,.034),'Steel',.004)
for i in range(9):cube('Printed line',(-.017,-.019,.125-i*.026),(.21 if i%3 else .15,.001,.002),'Black',0)
save('Clipboard')
cube('Desktop',(0,0,.85),(2,.72,.065),'Wood',.024)
for x in [-.88,.88]:cube('Desk side',(x,0,.42),(.065,.64,.84),'Steel',.013)
cube('Privacy',(0,.3,.47),(1.76,.05,.68),'Enamel',.012)
for x in [-.55,.55]:cube('Drawer',(x,-.04,.69),(.7,.56,.22),'Wood',.012);rod('Pull',(x-.1,-.345,.68),(x+.1,-.345,.68),.014,'Brass')
save('ReceptionDesk')
# Small belongings on a table; no text or secret data.
cyl('Mug',(0,0,.061),.044,.12,'Ivory',vertices=32);cyl('Tea',(0,0,.122),.039,.003,'Black',vertices=32)
torus('Handle',(.053,0,.066),.031,.008,'Ivory',(math.pi/2,0,0))
for i in range(4):cube('Paper stack',(.22,.02,.006+i*.002),(.18,.26,.002),'Ivory',0,rot=(0,0,.06*i))
rod('Pen',(.29,-.05,.02),(.23,.1,.02),.005,'Black')
save('TableBelongings')
cube('Backplate',(0,0,0),(.13,.035,.25),'Brass',.015);rod('Arm',(0,-.03,-.04),(0,-.20,-.04),.017,'Brass');rod('Stem',(0,-.20,-.04),(0,-.20,.08),.017,'Brass')
bpy.ops.mesh.primitive_cone_add(vertices=32,radius1=.16,radius2=.1,depth=.2,location=(0,-.2,.15));o=bpy.context.object;o.data.materials.append(mats['Ivory']);parts.append(o)
save('WallSconce')
for o in bpy.data.objects:o.hide_set(False)
bpy.ops.wm.save_as_mainfile(filepath=os.path.abspath('ArtSource/Blender/Finish/EchoFinishDetails.blend'))
