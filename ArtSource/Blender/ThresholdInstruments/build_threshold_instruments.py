"""Two original static enclosures around existing Threshold interactions.
Blender 5.2 LTS, metres. No screens, new indicators, text or mechanisms exported.
Source X width, -Y front, Z up. Current Unity importer mirrors X: see README.
"""
import bpy,os,json,math
from mathutils import Vector
BASE=os.path.dirname(os.path.abspath(__file__));OUT=os.path.join(BASE,'exports');PRE=os.path.join(BASE,'previews')
for p in [OUT,PRE]:os.makedirs(p,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
VALUES={
 'TI_Enamel':{'color':(.264,.299,.264,1),'roughness':.55,'metallic':.12},
 'TI_DarkFace':{'color':(.035,.049,.042,1),'roughness':.42,'metallic':.2},
 'TI_Rubber':{'color':(.014,.020,.016,1),'roughness':.87,'metallic':0},
 'TI_FoldedSteel':{'color':(.17,.204,.177,1),'roughness':.48,'metallic':.68},
 'TI_Fastener':{'color':(.32,.343,.30,1),'roughness':.40,'metallic':.8},
 'TI_ExposedEdge':{'color':(.20,.229,.20,1),'roughness':.55,'metallic':.65},
 'TI_UseMark':{'color':(.09,.098,.075,1),'roughness':.76,'metallic':.1}}
mats={}
for name,data in VALUES.items():
 m=bpy.data.materials.new(name);m.use_nodes=True;m.diffuse_color=data['color'];bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=data['color'];bs.inputs['Roughness'].default_value=data['roughness'];bs.inputs['Metallic'].default_value=data['metallic'];mats[name]=m
parts=[];roots=[];manifest={}
def material(o,n):o.data.materials.append(mats[n]);parts.append(o);return o
def weighted(o):
 for f in o.data.polygons:f.use_smooth=True
 bpy.context.view_layer.objects.active=o;mod=o.modifiers.new('Weighted fabricated normals','WEIGHTED_NORMAL');mod.keep_sharp=True;bpy.ops.object.modifier_apply(modifier=mod.name)
def cube(name,p,s,mat='TI_Enamel',bevel=.004,rot=(0,0,0)):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p,rotation=rot);o=bpy.context.object;o.name=name;o.dimensions=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);material(o,mat)
 if bevel:
  mod=o.modifiers.new('Folded edge radius','BEVEL');mod.width=bevel;mod.segments=3;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name);weighted(o)
 return o
def cylinder(name,p,r,depth,mat='TI_Fastener',axis=(0,-1,0),vertices=24):
 bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=depth,location=p);o=bpy.context.object;o.name=name;o.rotation_euler=Vector(axis).to_track_quat('Z','Y').to_euler();material(o,mat)
 for f in o.data.polygons:f.use_smooth=len(f.vertices)==4
 return o
def screw(p,r=.0065):
 cylinder('Captive screw head',p,r,.003,'TI_Fastener')
 cube('Screw slot',(p[0],p[1]-.00165,p[2]),(r*1.35,.0005,.0013),'TI_Rubber',.0003)
def cut(target,cutter):
 bpy.context.view_layer.objects.active=target;mod=target.modifiers.new('Actual recessed opening','BOOLEAN');mod.operation='DIFFERENCE';mod.solver='EXACT';mod.object=cutter;bpy.ops.object.modifier_apply(modifier=mod.name);parts.remove(cutter);bpy.data.objects.remove(cutter,do_unlink=True);weighted(target)
def bezel(name,centre,w,h,band,depth,mat='TI_FoldedSteel'):
 x,y,z=centre
 for side in [-1,1]:cube(name+' vertical',(x+side*(w-band)/2,y,z),(band,depth,h),mat,min(.002,band*.25))
 for side in [-1,1]:cube(name+' horizontal',(x,y,z+side*(h-band)/2),(w-2*band,depth,band),mat,min(.002,band*.25))
def beam(name,a,b,width,depth,mat='TI_FoldedSteel'):
 a=Vector(a);b=Vector(b);d=b-a;o=cube(name,(a+b)*.5,(width,depth,d.length),mat,.003);o.rotation_euler=d.to_track_quat('Z','Y').to_euler();return o
def join(name,items):
 bpy.ops.object.select_all(action='DESELECT')
 for o in items:o.select_set(True)
 bpy.context.view_layer.objects.active=items[0];bpy.ops.object.join();o=bpy.context.object;o.name=name;scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(angle_limit=1.1519,island_margin=.015,scale_to_bounds=True);bpy.ops.object.mode_set(mode='OBJECT');o.data.uv_layers.active.name='UVMap';return o
def export(name,groups,anchors,notes):
 global parts
 root=bpy.data.objects.new('EP_TI_'+name,None);bpy.context.collection.objects.link(root)
 objects=[root]
 for group,items in groups:o=join(group,items);o.parent=root;objects.append(o)
 anchors=dict(anchors)
 anchors.update({'AXIS_X':[.10,0,0],'AXIS_Y':[0,.10,0],'AXIS_Z':[0,0,.10],'FRONT_BASE':[0,0,0],'FRONT_TIP':[0,-.10,0]})
 anchor_aliases={key:name+'__'+key for key in anchors}
 anchors={anchor_aliases[key]:position for key,position in anchors.items()}
 for aname,position in anchors.items():
  o=bpy.data.objects.new(aname,None);bpy.context.collection.objects.link(o);o.parent=root;o.location=position;o.empty_display_type='PLAIN_AXES';o.empty_display_size=.025;objects.append(o)
 collection=bpy.data.collections.new('Asset / '+name);scene.collection.children.link(collection)
 for o in objects:
  for c in list(o.users_collection):c.objects.unlink(o)
  collection.objects.link(o)
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.context.view_layer.objects.active=root;file='exports/EP_TI_'+name+'.fbx'
 bpy.ops.export_scene.fbx(filepath=os.path.join(BASE,file),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH','EMPTY'},bake_anim=False,path_mode='RELATIVE')
 mesh_objects=[o for o in objects if o.type=='MESH'];coords=[o.matrix_world@v.co for o in mesh_objects for v in o.data.vertices];bounds=[[min(v[i] for v in coords),max(v[i] for v in coords)] for i in range(3)]
 group_data=[]
 for o in mesh_objects:
  cs=[o.matrix_world@v.co for v in o.data.vertices];bb=[[min(v[i] for v in cs),max(v[i] for v in cs)] for i in range(3)]
  group_data.append({'name':o.name,'pivot':[0,0,0],'bounds_blender_xyz':bb,'dimensions_width_depth_height':[b-a for a,b in bb],'vertices':len(o.data.vertices),'triangles':sum(len(f.vertices)-2 for f in o.data.polygons),'uv_layers':[uv.name for uv in o.data.uv_layers]})
 manifest[name]={'file':file,'root':'Existing instrument body centre, not floor','bounds_blender_xyz':bounds,'dimensions_width_depth_height':[b-a for a,b in bounds],'meshes':group_data,'materials':sorted({m.name for o in mesh_objects for m in o.data.materials}),'anchor_aliases':anchor_aliases,'anchors_blender_xyz':anchors,'notes':notes}
 for o in objects:o.hide_render=True;o.hide_set(True)
 roots.append(root);parts=[];print('EXPORTED',name,manifest[name]['dimensions_width_depth_height'],flush=True)

# Wall review unit. Envelope .88 wide x .10 deep x .68 high around the old origin.
body=cube('Wall pressed enclosure',(0,0,0),(.88,.10,.68),'TI_Enamel',.018)
cutter=cube('Wall face clearance',(0,-.05,0),(.806,.090,.584),'TI_Rubber',.013);cut(body,cutter)
cube('Recessed perimeter gasket',(0,-.033,0),(.801,.012,.579),'TI_Rubber',.011)
cube('Single unobstructed instruction face',(0,-.0385,0),(.784,.010,.561),'TI_DarkFace',.009)
for x in [-.412,.412]:
 for z in [-.303,.303]:
  cutter=cylinder('Recess for captive screw',(x,-.048,z),.0083,.018,'TI_Rubber');cut(body,cutter);screw((x,-.0475,z))
# Side join and service seam belong to the enclosure, never cross the existing copy.
for x in [-.44,.44]:cube('Side lid seam',(x*.997,.024,0),(.0018,.0018,.60),'TI_Rubber',0)
for z in [-.185,-.15,-.115]:cube('Side recessed ventilation',( .439,.005,z),(.0016,.048,.009),'TI_Rubber',.001)
# Restrained rubbed corners and lower edge, actual millimetre-scale marks.
for x,length,z in [(-.32,.047,-.329),(-.258,.028,-.329),(.341,.021,-.328)]:cube('Rubbed lower lip',(x,-.043,z),(length,.001,.002),'TI_ExposedEdge',0)
cube('Small lower handling mark',(.335,-.046,-.312),(.018,.0007,.004),'TI_UseMark',.001)
wall=parts.copy()
export('WallReviewHousing',[('WallHousing',wall)],{'ANCHOR_EXISTING_TITLE':[0,-.061,.20],'ANCHOR_EXISTING_INSTRUCTIONS':[0,-.061,-.07],'ANCHOR_FRONT_PLANE':[0,-.05,0]},'Wall body matches existing Unity .10 depth x .68 high x .88 wide. Entire central face stays clear for current title and instruction labels. No new pilot, button or screen. Root aligns to (2.35,1.45,-75.7); corrected importer X plus placement yaw270 faces -UnityX.')

# Desk conservation housing. Existing title/pilot positions remain unchanged.
body=cube('Desk folded instrument case',(0,0,0),(.38,.12,.34),'TI_Enamel',.013)
cutter=cube('Desk face clearance',(0,-.064,0),(.324,.07,.281),'TI_Rubber',.009);cut(body,cutter)
cube('Desk face gasket',(0,-.0405,0),(.320,.017,.277),'TI_Rubber',.008)
cube('Desk replaceable operation face',(0,-.0495,0),(.305,.011,.262),'TI_DarkFace',.006)
# Title stays on a single broad pressed surface; no simulated additional controls.
for x in [-.169,.169]:
 for z in [-.147,.147]:
  cutter=cylinder('Recess for desk screw',(x,-.058,z),.0065,.018,'TI_Rubber');cut(body,cutter);screw((x,-.0575,z),.005)
# Open lamp recess surrounds the EXISTING 0.08 x 0.025 m Unity pilot.
# No lens/emissive surface is exported: current gameplay still owns that lamp.
bezel('Pilot aperture bezel',(0,-.0575,-.07),.130,.059,.013,.005,'TI_FoldedSteel')
cube('Pilot dark recess',(0,-.056,-.07),(.099,.001,.032),'TI_Rubber',.003)
for x in [-.171,.171]:cube('Desk rear lid seam',(x,.05,0),(.002,.002,.278),'TI_Rubber',0)
for x in [-.1,-.05,0,.05,.1]:cube('Rear ventilation cutout',(x,.0595,-.08),(.024,.001,.032),'TI_Rubber',.003)
for x,length in [(-.13,.036),(-.082,.016)]:cube('Desk worn lower rim',(x,-.058,-.149),(length,.001,.002),'TI_ExposedEdge',0)
housing=parts.copy();parts=[]
# Optional rear cantilever lets the original forward face rest on the higher counter.
# Separate mesh so the integrator can hide/fit it to the actual tabletop.
for x in [-.125,.125]:
 beam('Rear folded support arm',(x,.058,-.10),(x,.235,-.202),.028,.025)
 cube('Housing stand attachment',(x,.062,-.105),(.045,.011,.075),'TI_FoldedSteel',.003)
 cylinder('Stand joint fastener',(x,.069,-.105),.008,.005,'TI_Fastener',(0,1,0),16)
cube('Rear support foot',(0,.235,-.210),(.32,.17,.020),'TI_FoldedSteel',.008)
for x in [-.125,.125]:
 for y in [.18,.29]:cube('Foot rubber isolator',(x,y,-.222),(.041,.040,.004),'TI_Rubber',.005)
for x in [-.115,.115]:cylinder('Foot mounting screw',(x,.242,-.198),.009,.004,'TI_Fastener',(0,0,1),16)
stand=parts.copy();parts=housing+stand
export('DeskPreserveHousing',[('DeskHousing',housing),('OptionalRearStand',stand)],{'ANCHOR_EXISTING_TITLE':[0,-.071,.05],'ANCHOR_EXISTING_PILOT':[0,-.072,-.07],'ANCHOR_PILOT_FRONT':[0,-.079,-.07],'ANCHOR_TABLE_CONTACT':[0,.235,-.224],'ANCHOR_FRONT_PLANE':[0,-.06,0]},'DeskHousing body matches old .12 depth x .34 high x .38 wide. Root aligns to (2.12,1.08,-82.6), corrected import X then yaw270. Existing ORIGINAL title and pilot remain entirely in front of all new geometry. OptionalRearStand extends rearwards +Y and to Z=-.224: contact would be world (2.355,.856,-82.6), matching the nominal main counter height. Verify the actual tabletop; remove/adjust only this mesh if needed. No exported indicator, screen or additional interaction.')

axis='Source X width, -Y front, Z up. Current Unity importer (bakeAxisConversion 0) maps (-X,Z,-Y). Negate imported root localScale.x once, retain imported rotation and scale magnitudes, then apply placementRotation * importedRotation; corrected assembly map (X,Z,-Y). Both instruments use placement yaw270 to face -UnityX. Check FRONT_TIP - FRONT_BASE using TransformPoint positions, not transform.forward under a mirrored parent.'
with open(os.path.join(BASE,'manifest.json'),'w') as f:json.dump({'date':'2026-10-01','authorship':'Original project-authored meshes; no external models/textures/fonts/downloads/paid services. Preview labels are not exported.','axis_contract':axis,'materials':VALUES,'assets':manifest},f,indent=2)
for o in [roots[0]]+list(roots[0].children_recursive):o.hide_set(False);o.hide_render=False
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BASE,'EchoThresholdInstruments.blend'))
for o in [roots[0]]+list(roots[0].children_recursive):o.hide_set(True);o.hide_render=True

# Isolated studio review. The labelled examples are only spatial clearance checks.
scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True;scene.render.resolution_x=1200;scene.render.resolution_y=1000;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX';scene.world.color=(.09,.09,.09)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.345));ground=bpy.context.object;ground.name='Preview only ground';gm=bpy.data.materials.new('Preview ground');gm.diffuse_color=(.10,.105,.092,1);ground.data.materials.append(gm)
for name,pos,power,size,color in [('Key',(-1.5,-2,2),180,2,(1,.85,.67)),('Fill',(2,-1,1),100,2,(.68,.80,1)),('Rim',(0,2,2),140,2,(1,.9,.72))]:
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.shape='DISK';d.size=size;d.color=color;o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o);o.location=pos;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();camera=bpy.context.object;scene.camera=camera
preview_material=bpy.data.materials.new('Preview existing ivory copy');preview_material.use_nodes=True;preview_material.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.68,.72,.63,1)
pilot_material=bpy.data.materials.new('Preview existing pilot');pilot_material.use_nodes=True;pilot_material.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.10,.57,.44,1)
labels=[]
def text_sample(name,words,position,width,size):
 curve=bpy.data.curves.new(name,'FONT');curve.body=words;curve.size=size;curve.align_x='CENTER';curve.align_y='CENTER';curve.space_line=1.3;o=bpy.data.objects.new(name,curve);bpy.context.collection.objects.link(o);o.location=position;o.rotation_euler=(math.pi/2,0,0);curve.materials.append(preview_material);bpy.context.view_layer.update()
 if o.dimensions.x>width:o.scale*=width/o.dimensions.x
 labels.append(o)
def show(index):
 for i,root in enumerate(roots):
  for o in [root]+list(root.children_recursive):o.hide_set(i!=index);o.hide_render=i!=index
def render(name,pos,target=(0,0,0),lens=52):
 camera.location=pos;camera.rotation_euler=(Vector(target)-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.lens=lens;scene.render.filepath=os.path.join(PRE,name+'.png');bpy.ops.render.render(write_still=True)
show(0);render('WallReviewHousing',(-.78,-1.48,.48))
text_sample('Preview existing wall title','6B / REVISIÓN',(0,-.061,.20),.70,.049)
text_sample('Preview existing wall instructions','CORREDOR LATERAL\nREGISTRÁ TU LLEGADA\nMIRÁ LA PUERTA. ESPERÁ.\nR: VOLVER / RESTABLECER',(0,-.061,-.07),.74,.030)
render('Wall-label-clearance',(0,-1.52,.015),lens=58)
for o in labels:bpy.data.objects.remove(o,do_unlink=True)
labels=[];show(1);ground.location.z=-.225
render('DeskPreserveHousing',(-.62,-.81,.31),target=(0,.06,-.015),lens=54)
text_sample('Preview existing ORIGINAL','ORIGINAL',(0,-.071,.05),.27,.031)
bpy.ops.mesh.primitive_cube_add(size=1,location=(0,-.072,-.07));preview_pilot=bpy.context.object;preview_pilot.name='Preview existing Unity pilot - not exported';preview_pilot.dimensions=(.08,.014,.025);preview_pilot.data.materials.append(pilot_material)
render('Desk-label-pilot-clearance',(0,-.82,.014),target=(0,0,-.015),lens=60)
print('THRESHOLD INSTRUMENTS COMPLETE',OUT,flush=True)
