"""Adult display surface, derived from the pinned MakeHuman CC0 base.

The rig data is used only to bake this authored resting pose. No skeleton,
animation, gesture, gaze tracking or audio is exported as an Echo channel.
"""
import bpy, bmesh, math, json, hashlib
from pathlib import Path
from collections import defaultdict
from mathutils import Vector, Matrix

ROOT=Path('/Users/lautigr116/EchoProtocol')
OUT=ROOT/'ArtSource/Blender/Reconstruction'
SRC=ROOT/'ArtSource/ThirdParty/MakeHumanBase'
(OUT/'exports').mkdir(exist_ok=True)
(OUT/'renders').mkdir(exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
raw=[];groups=defaultdict(list);group='';uvs=[]
for line in (SRC/'base.obj').read_text().splitlines():
    t=line.split()
    if not t:continue
    if t[0]=='v':raw.append(Vector(tuple(map(float,t[1:4]))))
    elif t[0]=='g':group=t[1]
    elif t[0]=='f':groups[group].append([int(x.split('/')[0])-1 for x in t[1:]])
# Two official adult age deltas give a mature jaw, brow and nasal structure.
# Coefficients are authored; no real individual is represented.
canonical=[v.copy() for v in raw]
for age,amount in [('young',.65),('old',.35)]:
    for line in (SRC/('caucasian-male-'+age+'.target')).read_text().splitlines():
        t=line.split()
        if len(t)==4 and not line.startswith('#'):
            raw[int(t[0])]+=Vector(tuple(map(float,t[1:])))*amount
bodyIds={i for f in groups['body'] for i in f}
ground=min(raw[i].y for i in bodyIds)
S=1.80/(max(raw[i].y for i in bodyIds)-ground)
def convert(v):return Vector((v.x*S,-v.z*S,(v.y-ground)*S))
verts=[convert(v) for v in raw]
raw=canonical
skel=json.loads((SRC/'default.mhskel').read_text())
weights=json.loads((SRC/'default_weights.mhw').read_text())['weights']
def joint(name):
    ids=skel['joints'][name]
    return sum((verts[i] for i in ids),Vector())/len(ids)
def pivot(c,q):return Matrix.Translation(c)@q.to_matrix().to_4x4()@Matrix.Translation(-c)
changes={}
for side,sign in [('L',1),('R',-1)]:
    a=joint('upperarm01.'+side+'____head');e=joint('lowerarm01.'+side+'____head');w=joint('wrist.'+side+'____head')
    # Relaxed narrow upper arms; slightly different forearm pitch on each side.
    direction=Vector((sign*.19,.03,-1)).normalized()
    q=(e-a).rotation_difference(direction)
    arm=pivot(a,q);ne=arm@e
    target=Vector((sign*.045,-.08 if sign>0 else .025,-1)).normalized()
    fore=pivot(ne,(arm@w-ne).rotation_difference(target))@arm
    changes['upperarm01.'+side]=arm;changes['upperarm02.'+side]=arm
    changes['lowerarm01.'+side]=fore;changes['lowerarm02.'+side]=fore
    # Narrow the excessive base stance without translating recorded roots.
    hip=joint('upperleg01.'+side+'____head');ankle=joint('foot.'+side+'____head')
    target=Vector((sign*.145,ankle.y,ankle.z))-hip
    leg=pivot(hip,(ankle-hip).rotation_difference(target))
    changes['upperleg01.'+side]=leg
def bone_matrix(name):
    if name in changes:return changes[name]
    b=skel['bones'].get(name)
    return bone_matrix(b['parent']) if b and b['parent'] else Matrix.Identity(4)
posed=[Vector() for _ in verts];total=[0.0]*len(verts)
for name,entries in weights.items():
    m=bone_matrix(name)
    for i,w in entries:posed[i]+=m@verts[i]*w;total[i]+=w
for i in range(len(verts)):
    posed[i]=posed[i]/total[i] if total[i]>0 else verts[i].copy()
    # Very mild adult craniofacial asymmetry and a firmer lower jaw.
    p=posed[i]
    if raw[i].y>5.9:
        jaw=math.exp(-((raw[i].y-6.35)/.42)**2)
        p.x*=1+.055*jaw
        if raw[i].z>1.0:p.y-=.0015*jaw
        p.x+=.0015*math.exp(-((raw[i].y-7.6)/.9)**2)

palette={
 'FIG_Skin':(.38,.29,.23,1),'FIG_Hair':(.044,.036,.030,1),
 'FIG_Jacket':(.12,.15,.145,1),'FIG_Trousers':(.068,.079,.081,1),
 'FIG_Shoe':(.049,.044,.038,1),'FIG_Sole':(.024,.022,.020,1),
 'FIG_Stitch':(.20,.21,.18,1),'FIG_Metal':(.16,.17,.16,1),
 'FIG_Eye':(.42,.40,.35,1),'FIG_Iris':(.072,.084,.078,1)}
mats={}
for name,color in palette.items():
    m=bpy.data.materials.new(name);m.diffuse_color=color;m.use_nodes=True
    bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=color
    bs.inputs['Roughness'].default_value=.8 if name!='FIG_Eye' else .42
    mats[name]=m
parts=[]
def mesh(name,ids,faces,mat,sub=0,inflate=0):
    remap={v:i for i,v in enumerate(ids)}
    me=bpy.data.meshes.new(name);me.from_pydata([posed[i] for i in ids],[],[[remap[i] for i in f] for f in faces]);me.update()
    o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o);o.data.materials.append(mats[mat]);parts.append(o)
    bpy.context.view_layer.objects.active=o;o.select_set(True)
    for p in me.polygons:p.use_smooth=True
    if inflate:
        for v in me.vertices:v.co+=v.normal*inflate
    if sub:
        md=o.modifiers.new('Anatomical smoothing','SUBSURF');md.levels=sub;bpy.ops.object.modifier_apply(modifier=md.name)
    o.select_set(False);return o
def grouped(name,faces,mat,sub=0,inflate=0):return mesh(name,sorted({i for f in faces for i in f}),faces,mat,sub,inflate)

# Only exposed anatomy is exported; all torso/pelvic anatomy is discarded.
exposed=[f for f in groups['body'] if min(raw[i].y for i in f)>5.57 or (min(abs(raw[i].x) for i in f)>4.03 and max(raw[i].y for i in f)<3.13)]
skin=grouped('Adult face neck and articulated hands',exposed,'FIG_Skin',1)
# Use the official outer garment helper as a continuous, clothed silhouette.
cloth=groups['helper-tights']
jacket=[]
for f in cloth:
    c=sum((raw[i] for i in f),Vector())/len(f)
    if abs(c.x)>4.12 and c.y<3.1:continue
    if c.y>-7.49:jacket.append(f)
jacketObj=grouped('Continuous work jacket',jacket,'FIG_Jacket',2,.005)
jacketObj.data.materials.append(mats['FIG_Trousers'])
bm=bmesh.new();bm.from_mesh(jacketObj.data)
bmesh.ops.bisect_plane(bm,geom=list(bm.verts)+list(bm.edges)+list(bm.faces),dist=.00001,plane_co=(0,0,1.015),plane_no=(0,0,1),clear_inner=False,clear_outer=False)
bm.to_mesh(jacketObj.data);bm.free()
for poly in jacketObj.data.polygons:
    center=sum((jacketObj.data.vertices[i].co for i in poly.vertices),Vector())/len(poly.vertices)
    if center.z<1.015 and abs(center.x)<.215:poly.material_index=1
# Add restrained cloth folds to catch grazing light; do not sculpt muscles into cloth.
for o in [jacketObj]:
    for v in o.data.vertices:
        p=v.co
        if p.z>1.015:
            bend=math.exp(-((p.z-1.18)/.20)**2)*math.sin(p.z*76+p.x*32)
            if abs(p.x)>.18:p.y+=.0026*bend
            elif p.y<-.06:p.y+=.0018*math.sin(p.z*57+abs(p.x)*29)
        else:
            p.y+=.0021*math.sin(p.z*69+p.x*31)*math.exp(-((p.z-.55)/.18)**2)
            if .12<p.z<.85:
                center=.143 if p.x>0 else -.143
                p.x=center+(p.x-center)*(1+.17*math.sin(math.pi*(p.z-.12)/.73)**2)

def sphere(name,center,size,mat,segments=32,rings=16):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments,ring_count=rings,location=center)
    o=bpy.context.object;o.name=name;o.scale=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    o.data.materials.append(mats[mat]);parts.append(o)
    for p in o.data.polygons:p.use_smooth=True
    return o
eyeCenters={}
for side,sign in [('L',1),('R',-1)]:
    ec=sum((posed[i] for f in groups['joint-'+side.lower()+'-eye'] for i in f),Vector())/sum(len(f) for f in groups['joint-'+side.lower()+'-eye'])
    eyeCenters[sign]=ec
    sphere('Quiet anatomical eye '+side,ec,(.0145,.0145,.0145),'FIG_Eye')
    sphere('Small iris '+side,ec+Vector((0,-.014,0)),(.0052,.0012,.0052),'FIG_Iris',24,12)
    sphere('Pupil '+side,ec+Vector((0,-.015,0)),(.0022,.0006,.0022),'FIG_Hair',20,10)
    # Broad low leather shoes enclose feet and the trouser hem.
    foot=changes['upperleg01.'+side]@joint('foot.'+side+'____head')
    sphere('Leather shoe '+side,(foot.x,foot.y-.074,.050),(.059,.139,.047),'FIG_Shoe')
    sphere('Rubber sole '+side,(foot.x,foot.y-.077,.020),(.061,.143,.018),'FIG_Sole')

# A close-cropped receding cap follows the actual cranial topology.
hairfaces=[]
for f in groups['body']:
    c=sum((raw[i] for i in f),Vector())/len(f)
    threshold=7.34 if c.z<.25 else 7.91+.22*math.exp(-((abs(c.x)-.48)/.22)**2)
    if min(raw[i].y for i in f)>threshold:hairfaces.append(f)
hair=grouped('Close cropped receding hair',hairfaces,'FIG_Hair',1,.0019)
bm=bmesh.new();bm.from_mesh(hair.data)
edge=[v for v in bm.verts if v.is_boundary]
for _ in range(14):
    smooth={v:sum((e.other_vert(v).co for e in v.link_edges if e.other_vert(v).is_boundary),Vector())/max(1,sum(e.other_vert(v).is_boundary for e in v.link_edges)) for v in edge}
    for v,c in smooth.items():v.co=v.co.lerp(c,.38)
bm.to_mesh(hair.data);bm.free()
def curve(name,points,depth,mat):
    cu=bpy.data.curves.new(name,'CURVE');cu.dimensions='3D';cu.bevel_depth=depth;cu.bevel_resolution=2
    sp=cu.splines.new('POLY');sp.points.add(len(points)-1)
    for p,q in zip(sp.points,points):p.co=(*q,1)
    o=bpy.data.objects.new(name,cu);bpy.context.collection.objects.link(o);o.data.materials.append(mats[mat]);parts.append(o);return o
# Quiet brows follow the actual skull surface.
from mathutils.bvhtree import BVHTree
bpy.context.view_layer.update();skinTree=BVHTree.FromObject(skin,bpy.context.evaluated_depsgraph_get())
for sign in [-1,1]:
    points=[]
    ec=eyeCenters[sign]
    for dx,dz in [(-.016,.014),(-.004,.020),(.008,.021),(.020,.016)]:
        x=abs(ec.x)+dx;z=ec.z+dz
        hit=skinTree.ray_cast(Vector((sign*x,-1,z)),Vector((0,1,0)))[0]
        points.append((sign*x,hit.y-.0013,z))
    curve('Natural brow',points,.00125,'FIG_Hair')

def patch(name,points,mat,thick=.0015):
    me=bpy.data.meshes.new(name);me.from_pydata(points,[],[tuple(range(len(points)))]);me.update()
    o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o);o.data.materials.append(mats[mat]);parts.append(o)
    bpy.context.view_layer.objects.active=o;o.select_set(True);md=o.modifiers.new('Tailored thickness','SOLIDIFY');md.thickness=thick;bpy.ops.object.modifier_apply(modifier=md.name);o.select_set(False);return o
# Shirt collar and seam details are project-authored, scaled to the actual body.
bpy.context.view_layer.update();tree=BVHTree.FromObject(jacketObj,bpy.context.evaluated_depsgraph_get())
def oncloth(x,z,offset=.0015):
    hit=tree.ray_cast(Vector((x,-1,z)),Vector((0,1,0)))[0]
    return Vector((x,hit.y-offset if hit else -.14,z))
curve('Jacket center seam',[oncloth(0,.98+i*.014) for i in range(41)],.0010,'FIG_Stitch')
for z in [1.39,1.28,1.17,1.06]:sphere('Small jacket button',oncloth(0,z,.003),(.0037,.002,.0037),'FIG_Metal',12,8)
patch('Chest pocket',[oncloth(x,z,.003) for x,z in [(.066,1.389),(.135,1.389),(.135,1.326),(.066,1.326)]],'FIG_Jacket')
curve('Pocket welt',[oncloth(x,1.389,.005) for x in [.066,.08,.10,.12,.135]],.0012,'FIG_Stitch')

# Freeze all authored pieces into a single mesh with named material slots.
bpy.ops.object.select_all(action='DESELECT')
for o in parts:
    o.select_set(True);bpy.context.view_layer.objects.active=o
    if o.type=='CURVE':bpy.ops.object.convert(target='MESH')
bpy.context.view_layer.objects.active=skin;bpy.ops.object.join();o=skin;o.name='EP_AdultRecordedSurface'
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
bpy.ops.export_scene.fbx(filepath=str(OUT/'exports/EP_ReconstructedAdult.fbx'),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH'},bake_anim=False)

# Neutral review light intentionally exposes anatomy rather than hiding it.
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=48
scene.world.color=(.10,.10,.10)
def area(name,pos,energy,size,color):
    d=bpy.data.lights.new(name,'AREA');d.energy=energy;d.shape='DISK';d.size=size;d.color=color
    l=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(l);l.location=pos;l.rotation_euler=(Vector((0,0,1.35))-l.location).to_track_quat('-Z','Y').to_euler()
area('Large soft key',(-1.4,-2.2,2.5),180,2.0,(.83,.91,1))
area('Warm practical edge',(1.7,.7,2.0),140,1.0,(1,.74,.49))
d=bpy.data.cameras.new('Review camera');cam=bpy.data.objects.new('Review camera',d);bpy.context.collection.objects.link(cam);scene.camera=cam;d.lens=62
scene.render.resolution_x=960;scene.render.resolution_y=1200;scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'
for name,loc,target in [('front',(0,-3.35,1.2),(0,0,.96)),('near',(.29,-.93,1.76),(0,-.05,1.62)),('profile',(1.0,-.13,1.76),(0,0,1.63))]:
    cam.location=loc;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();scene.render.filepath=str(OUT/'renders'/f'{name}.png');bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'EchoReconstructedAdult.blend'))
manifest={'date':'2026-10-02','source':'MakeHuman official CC0 base and graphical weights, pinned in ArtSource/ThirdParty/MakeHumanBase/provenance.json','authored':'Mature adult morphology (65% young / 35% old), static resting pose, narrow stance, craniofacial asymmetry, cropped hair, anatomical eyes, continuous clothed stand-collar surface and tailored details','model':'exports/EP_ReconstructedAdult.fbx','vertices':len(o.data.vertices),'triangles':sum(len(p.vertices)-2 for p in o.data.polygons),'materials':[m.name for m in o.data.materials],'bounds':[[min(v.co[k] for v in o.data.vertices),max(v.co[k] for v in o.data.vertices)] for k in range(3)],'rig':False,'animations':False,'recordedChannels':['root position','body yaw'],'sha256':hashlib.sha256((OUT/'exports/EP_ReconstructedAdult.fbx').read_bytes()).hexdigest()}
(OUT/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n');print(json.dumps(manifest))
