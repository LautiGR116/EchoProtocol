"""Original secondary furniture; shares dimensions/materials with campaign kit."""
import pathlib
base=pathlib.Path('ArtSource/Blender/Campaign/build_campaign_kit.py').read_text()
exec(base.split('# Blender axes')[0])
cube('Top',(0,0,.72),(.94,.64,.06),'Wood');cube('Drawer',(0,0,.6),(.85,.56,.18),'Wood')
for x in [-.37,.37]:
 for y in [-.23,.23]:cube('Leg',(x,y,.3),(.055,.055,.6),'Wood')
rod('Handle',(-.10,-.32,.6),(.10,-.32,.6),.012,'Brass');save('TelephoneTable')
# Bookshelf: varied book heights and subtly tilted spines, not textures pretending to be geometry.
for x in [-.65,.65]:cube('Side',(x,0,1.05),(.075,.35,2.1),'Wood')
cube('Back',(0,.165,1.05),(1.3,.035,2.1),'Wood')
for z in [.08,.58,1.08,1.58,2.08]:cube('Shelf',(0,0,z),(1.35,.36,.055),'Wood')
for row in range(4):
 for i in range(13):
  height=.28+(i*7%5)*.025;x=-.54+i*.083;z=.11+row*.5+height/2
  cube('Book',(x,-.045,z),(.06,.23,height),['Fabric','Ivory','Enamel','Wood'][i%4],.004,rot=(0,(-.025 if i%3 else .055),0))
  for dz in [-height*.3,height*.3]:cube('Spine band',(x,-.163,z+dz),(.06,.008,.011),'Brass',.001)
save('Bookshelf')
# Institutional clock at 03:17, an environmental prop, never an Echo timestamp.
cyl('Clock rim',(0,0,0),.24,.045,'Steel',(math.pi/2,0,0));cyl('Clock face',(0,-.027,0),.216,.012,'Ivory',(math.pi/2,0,0))
for i in range(12):
 a=i*math.pi/6;rod('Tick',(.18*math.sin(a),-.039,.18*math.cos(a)),(.201*math.sin(a),-.039,.201*math.cos(a)),.004,'Black')
for a,length in [(math.radians(98.5),.12),(math.radians(102),.18)]:rod('Hand',(0,-.05,0),(length*math.sin(a),-.05,length*math.cos(a)),.006,'Black')
save('Clock')
# Night window with double timber rebate, inset pane and draped side curtains.
cube('Night behind pane',(0,.06,0),(1.8,.045,1.7),'Black')
for x in [-.94,.94,0]:cube('Window mullion',(x,0,0),(.08,.18,1.85),'Wood')
for z in [-.90,0,.90]:cube('Window transom',(0,0,z),(1.96,.18,.08),'Wood')
for x in [-.47,.47]:
 for z in [-.44,.44]:cube('Glass pane',(x,-.015,z),(.84,.025,.8),'Glass',.001)
rod('Curtain rail',(-1.25,-.12,1.05),(1.25,-.12,1.05),.022,'Brass')
for side in [-1,1]:
 for i in range(7):
  x=side*(.93+i*.06);cube('Curtain fold',(x,-.14+math.sin(i*1.8)*.045,-.08),(.078,.05,2.16),'Fabric',.022)
save('DomesticWindow')
# Ceiling grilles, slats with real depth.
cube('Vent surround',(0,0,0),(.7,.45,.05),'Enamel')
cube('Vent recess',(0,0,-.03),(.62,.37,.012),'Black')
for i in range(9):cube('Slat',(0,-.16+i*.04,-.045),(.61,.016,.025),'Steel',.004)
save('CeilingVent')
for o in bpy.data.objects:o.hide_set(False)
bpy.ops.wm.save_as_mainfile(filepath=os.path.abspath('ArtSource/Blender/Campaign/EchoDressing.blend'))
