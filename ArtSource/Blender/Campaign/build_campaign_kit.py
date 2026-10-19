"""Original Echo Protocol campaign kit. Blender 5.x, metres, CC0 project-authored geometry."""
import bpy, math, os
from mathutils import Vector
OUT=os.path.abspath('Assets/_Game/Art/Models/Campaign')
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
# Blender axes X width, Y depth (front -Y), Z vertical.
cube('Steel leaf',(0,0,1.23),(1.35,.10,2.46),'Enamel')
for x in [-.74,.74]:cube('Rebated jamb',(x,0,1.28),(.13,.2,2.56))
cube('Header',(0,0,2.51),(1.61,.2,.13))
cube('Window rubber',(0,-.065,1.75),(.82,.055,.61),'Black')
cube('Wired pane',(0,-.097,1.75),(.74,.016,.53),'Glass',.002)
for x in range(-3,4):rod('Wire',(x*.095,-.11,1.49),(x*.095,-.11,2.01),.0025)
for z in range(5):rod('Wire',(-.37,-.11,1.54+z*.095),(.37,-.11,1.54+z*.095),.0025)
cube('Kick plate',(0,-.065,.27),(1.17,.016,.36))
cube('Handle plate',(.48,-.07,1.03),(.09,.02,.25),'Brass');rod('Handle',(.47,-.18,1.08),(.24,-.18,1.08),.025,'Brass')
save('ClinicalDoor')
# Fluorescent fixture
cube('Fixture',(0,0,0),(1.22,.34,.085),'Enamel')
for y in [-.095,.095]:cyl('Tube',(0,y,-.065),.026,1.1,'Ivory',(0,math.pi/2,0))
for x in [-.52,.52]:cube('End cap',(x,0,-.045),(.08,.31,.08))
save('Fluorescent')
# Lockers
cube('Cabinet',(0,0,1),(.94,.48,2),'Enamel')
for x in [-.235,.235]:
 cube('Inset door',(x,-.25,1.02),(.435,.025,1.91),'Steel')
 for z in [1.65,1.70,1.75, .28,.33,.38]:cube('Vent',(x,-.267,z),(.29,.008,.017),'Black',.002)
 cube('Tag',(x,-.277,1.42),(.18,.008,.07),'Ivory',.002);rod('Pull',(x+.13,-.3,.88),(x+.13,-.3,1.07),.012)
save('Locker')
# Examination trolley
cube('Mattress',(0,0,.83),(.73,1.9,.13),'Fabric',.045);cube('Frame',(0,0,.73),(.79,1.99,.10))
for x in [-.32,.32]:
 for y in [-.79,.79]:
  rod('Leg',(x,y,.14),(x,y,.72),.03);cyl('Wheel',(x,y,.10),.085,.04,'Black',(0,math.pi/2,0))
 for y in [-.7,.7]:rod('Rail support',(x,y,.73),(x,y,1.0),.015)
 rod('Side rail',(x,-.7,1),(x,.7,1),.018)
cube('Lower shelf',(0,0,.3),(.61,1.52,.035))
save('Trolley')
# Pump / valve with readable handwheel
cube('Pump base',(0,0,.12),(1.1,.72,.24))
cyl('Motor',(0,0,.55),.27,.85,'Enamel',(0,math.pi/2,0))
for x in [-.28,-.18,-.08,.02,.12,.22]:torus('Cooling fin',(x,0,.55),.27,.018,'Steel',(0,math.pi/2,0))
rod('Pressure pipe',(.55,0,.5),(.55,0,1.35),.07);rod('Elbow',(.55,0,1.35),(.1,0,1.35),.07)
torus('Handwheel',(.1,-.23,1.35),.21,.025,'Brass',(math.pi/2,0,0))
for angle in [0,math.pi/2,math.pi,math.pi*1.5]:rod('Spoke',(.1,-.23,1.35),(.1+math.cos(angle)*.2,-.23,1.35+math.sin(angle)*.2),.012,'Brass')
cyl('Gauge',(.55,-.095,1.05),.105,.055,'Ivory',(math.pi/2,0,0));rod('Needle',(.55,-.127,1.05),(.60,-.127,1.10),.006,'Black')
save('PressurePump')
# Table with drawer and lamp
cube('Top',(0,0,.72),(.78,.58,.065),'Wood')
cube('Drawer',(0,-.01,.6),(.68,.49,.2),'Wood');rod('Drawer pull',(-.08,-.28,.6),(.08,-.28,.6),.014,'Brass')
for x in [-.29,.29]:
 for y in [-.20,.20]:cube('Leg',(x,y,.29),(.055,.055,.58),'Wood')
cyl('Lamp foot',(0,0,.78),.13,.055,'Brass');rod('Lamp stem',(0,0,.8),(0,0,1.22),.022,'Brass')
bpy.ops.mesh.primitive_cone_add(vertices=32,radius1=.24,radius2=.13,depth=.29,location=(0,0,1.26));o=bpy.context.object;o.data.materials.append(mats['Ivory']);parts.append(o)
for angle in range(0,360,15):
 a=math.radians(angle);rod('Pleat',(.238*math.cos(a),.238*math.sin(a),1.115),(.132*math.cos(a),.132*math.sin(a),1.405),.002,'Wood')
save('LampTable')
# Upholstered armchair, individual cushions, timber supports
cube('Seat',(0,0,.44),(.74,.68,.19),'Fabric',.09)
cube('Back',(0,.30,.86),(.78,.20,.84),'Fabric',.09,(-.12,0,0))
for x in [-.45,.45]:
 cube('Arm',(x,0,.65),(.20,.81,.24),'Fabric',.075)
 for y in [-.27,.28]:cube('Leg',(x,y,.21),(.065,.075,.42),'Wood')
for x in [-.24,0,.24]:sphere('Tuft',(x,.175,1.04),(.018,.012,.018),'Wood')
save('Armchair')
# Rotary telephone
cube('Telephone',(0,0,.09),(.36,.27,.16),'Black',.065)
cyl('Dial',(0,-.045,.186),.095,.018,'Brass')
for i in range(10):
 a=i*math.pi*2/12; cyl('Finger hole',(.071*math.cos(a),-.045+.071*math.sin(a),.198),.013,.009,'Black',vertices=12)
rod('Handset',(-.16,.085,.225),(.16,.085,.225),.039,'Black')
for x in [-.16,.16]:sphere('Earpiece',(x,.085,.20),(.062,.06,.055),'Black')
for i in range(24):torus('Curled cord',(.22,.1-i*.014,.09),.018,.005,'Black',(math.pi/2,0,0))
save('Telephone')
# Filing cabinet
cube('Cabinet',(0,0,.74),(.58,.65,1.48),'Enamel')
for z in [.24,.57,.90,1.23]:
 cube('Drawer',(0,-.338,z),(.525,.035,.29),'Steel');cube('Label',(0,-.36,z+.07),(.17,.008,.055),'Ivory',.002);rod('Pull',(-.105,-.4,z-.02),(.105,-.4,z-.02),.012)
save('ArchiveCabinet')
# Kitchen furniture
cube('Cabinet',(0,0,.46),(1.8,.62,.92),'Enamel');cube('Counter',(0,0,.95),(1.86,.67,.06),'Ivory')
for x in [-.58,0,.58]:
 cube('Door',(x,-.33,.47),(.55,.025,.79),'Wood');rod('Handle',(x+.16,-.365,.7),(x+.16,-.365,.82),.014,'Brass')
cube('Sink rim',(-.43,0,.989),(.64,.46,.018),'Steel');cube('Sink dark',(-.43,0,1),(.53,.35,.01),'Black');rod('Tap',(-.43,.19,1),(-.43,.19,1.3),.022);rod('Tap spout',(-.43,.19,1.3),(-.43,0,1.3),.022)
save('Kitchen')
# Bed
cube('Frame',(0,0,.27),(1.48,2.09,.21),'Wood');cube('Mattress',(0,0,.46),(1.39,2,.21),'Ivory',.08);cube('Blanket',(0,-.32,.59),(1.4,1.31,.06),'Fabric',.04)
cube('Headboard',(0,1,.7),(1.53,.11,1.15),'Wood')
for x in [-.36,.36]:cube('Pillow',(x,.66,.61),(.58,.45,.13),'Ivory',.06,rot=(.05,0,.06 if x>0 else -.06))
save('Bed')
# Human coat silhouette with face intentionally non-specific; proportions adult.
sphere('Head',(0,0,1.62),(.105,.1,.14));cyl('Neck',(0,0,1.46),.045,.13,'Echo')
sphere('Coat chest',(0,0,1.22),(.215,.13,.29));cube('Coat hem',(0,0,.96),(.39,.24,.26),'Echo',.065)
for sign in [-1,1]:
 rod('Upper sleeve',(sign*.19,0,1.36),(sign*.26,-.015,1.08),.085,'Echo');rod('Lower sleeve',(sign*.26,-.015,1.08),(sign*.26,-.025,.86),.066,'Echo');sphere('Hand',(sign*.26,-.025,.80),(.048,.052,.079))
 rod('Trouser',(sign*.10,0,.9),(sign*.10,0,.45),.084,'Echo');rod('Calf',(sign*.10,0,.47),(sign*.10,0,.12),.06,'Echo');cube('Shoe',(sign*.10,-.055,.055),(.14,.27,.11),'Echo',.045)
# seams make the figure recognizable without pretending to record animation.
rod('Coat seam',(0,-.135,.85),(0,-.135,1.4),.006,'Steel')
save('RecordedFigure')
# preserve editable source for all meshes
for o in bpy.data.objects:o.hide_set(False)
bpy.ops.wm.save_as_mainfile(filepath=os.path.abspath('ArtSource/Blender/Campaign/EchoCampaignKit.blend'))
print('CAMPAIGN KIT COMPLETE',OUT)
