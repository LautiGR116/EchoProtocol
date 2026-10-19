"""Original periodic material bake, authored for Echo Protocol.

Run with Blender 5.2 --background --python this_file. All output stays beside
this script. The Overhaul kit is opened as a READ-ONLY reference in memory and
saved under a new name here; its source, exports and other agents' files remain
untouched. Mathematical height/albedo fields are baked to 8-bit PNG, then a
Blender Principled material and neutral preview scene are authored from them.
No source images, downloads, generated-image edits, external packages or AI are
used for these textures. NumPy ships with Blender.
"""
from pathlib import Path
import bpy, hashlib, json, math, os, struct, zlib
import numpy as np
from mathutils import Vector

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[2]
KIT = REPO / "ArtSource/Blender/Overhaul/EchoOverhaulKit.blend"
N = 1024
TAU = math.tau
u, v = np.meshgrid((np.arange(N)+.5)/N, (np.arange(N)+.5)/N)

def png(path, data):
    a = np.asarray(np.clip(np.round(data*255), 0, 255), dtype=np.uint8)
    if a.ndim == 2: a = a[:,:,None]
    height, width, channels = a.shape
    color = {1:0, 3:2, 4:6}[channels]
    def chunk(kind, payload):
        return struct.pack('>I', len(payload)) + kind + payload + struct.pack('>I', zlib.crc32(kind+payload)&0xffffffff)
    raw = b''.join(b'\0'+row.tobytes() for row in a)
    path.write_bytes(b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR',struct.pack('>IIBBBBB',width,height,8,color,0,0,0)) + chunk(b'IDAT',zlib.compress(raw,9)) + chunk(b'IEND',b''))

def smooth(x): return x*x*x*(x*(x*6-15)+10)

def noise(nx, ny, seed):
    # A periodic lattice sampled with quintic interpolation, not a wrapped crop.
    rng = np.random.default_rng(seed)
    grid = rng.random((ny,nx))
    x=u*nx; y=v*ny; ix=np.floor(x).astype(int)%nx; iy=np.floor(y).astype(int)%ny
    fx=smooth(x-np.floor(x)); fy=smooth(y-np.floor(y))
    a=grid[iy,ix]*(1-fx)+grid[iy,(ix+1)%nx]*fx
    b=grid[(iy+1)%ny,ix]*(1-fx)+grid[(iy+1)%ny,(ix+1)%nx]*fx
    return a*(1-fy)+b*fy

def srgb(linear):
    return np.where(linear <= .0031308, 12.92*linear, 1.055*np.maximum(linear,0)**(1/2.4)-.055)

def normals(height, metres_per_tile, relief_metres):
    # Image row zero is +V/top. Thus derivative in tangent +Y reverses image Y.
    dx=(np.roll(height,-1,axis=1)-np.roll(height,1,axis=1))*N*.5*relief_metres/metres_per_tile
    dy=-(np.roll(height,-1,axis=0)-np.roll(height,1,axis=0))*N*.5*relief_metres/metres_per_tile
    xyz=np.stack([-dx,-dy,np.ones_like(dx)],axis=-1)
    xyz/=np.linalg.norm(xyz,axis=-1,keepdims=True)
    return xyz*.5+.5

def walnut():
    # Long, slightly wandering growth bands and subtle vessels; no board seams,
    # knots, seams, nails, lighting, scratches or artificial black striping.
    broad=noise(5,3,2301); flow=noise(12,2,2302)
    warp=.44*np.sin(TAU*v)+.12*np.sin(TAU*(v*3+u*2))+.28*(flow-.5)
    phase=TAU*(26*u+warp)
    growth=(.5+.5*np.sin(phase))**9
    fine=(.5+.5*np.sin(TAU*(91*u+warp*2+.18*noise(25,7,2303))))**15
    fibers=noise(210,12,2304)
    vessels=np.maximum(0,noise(310,30,2305)-.66)**2
    value=.97+.17*(broad-.5)+.12*(flow-.5)-.095*growth-.037*fine+.035*(fibers-.5)-.35*vessels
    tint=np.array([.140,.083,.047])[None,None,:]
    color=np.clip(value[:,:,None]*tint,0,1)
    rough=np.clip(.405+.045*(broad-.5)+.050*growth+.035*(fibers-.5),.31,.55)
    height=.52+.13*(fibers-.5)-.14*growth-.05*fine-.35*vessels
    return color, rough, height, .60, .00018

def linen():
    # Plain weave: orthogonal threads alternate above/below each other. Contrast
    # stays restrained; silhouette folds belong to geometry, not the albedo.
    threads=96
    warpPhase=u*threads+.035*np.sin(TAU*v*4)
    weftPhase=v*threads+.030*np.sin(TAU*u*5)
    x=np.mod(warpPhase,1); y=np.mod(weftPhase,1)
    warp=np.maximum(0,np.cos((x-.5)*math.pi))**2
    weft=np.maximum(0,np.cos((y-.5)*math.pi))**2
    overUnder=np.cos(math.pi*warpPhase)*np.cos(math.pi*weftPhase)
    surface=warp*(.45+.18*overUnder)+weft*(.45-.18*overUnder)
    variation=noise(24,24,661)+.15*noise(180,180,662)
    slub=.007*(noise(7,5,663)-.5)
    value=.98+.022*(surface-.45)+.016*(variation-.58)+slub
    color=np.clip(value[:,:,None]*np.array([.50,.48,.44])[None,None,:],0,1)
    rough=np.clip(.91+.055*(1-surface)+.02*(variation-.5),.88,.98)
    height=.30+.12*surface+.012*(variation-.58)
    return color, rough, height, .18, .000055

manifest={'date':'2026-10-01','authorship':'Original deterministic procedural fields authored for Echo Protocol; no external image or texture source.',
    'source_kit_reference':str(KIT.relative_to(REPO)), 'source_kit_sha256_before':hashlib.sha256(KIT.read_bytes()).hexdigest(),
    'source_kit_use':'Read-only palette and Blender preview reference; original kit not changed.',
    'bake_method':'Periodic quintic lattice fields and explicit thread geometry sampled mathematically to PNG; tangent normal derived analytically by centered periodic differences.',
    'normal_convention':'Tangent-space OpenGL +Y; image-down derivative inverted. Unity import as Normal Map, no Flip Green.',
    'packing':'MetalSmooth: RGB=0 dielectric, alpha=1-roughness. Import non-sRGB. Roughness is non-sRGB. Diffuse is sRGB encoded without lighting.',
    'assets':{}}

for name,recipe in [('WalnutContinuous',walnut),('LinenPlain',linen)]:
    color,rough,height,tile,relief=recipe()
    maps={'Diffuse':srgb(color),'Roughness':rough,'NormalGL':normals(height,tile,relief),
          'MetalSmooth':np.dstack([np.zeros_like(rough)]*3+[1-rough]),'Height':height}
    rows=[]
    for channel,data in maps.items():
        path=ROOT/(name+'_'+channel+'.png');png(path,data)
        rows.append({'map':channel,'file':path.name,'bytes':path.stat().st_size,'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),
                     'min':float(data.min()),'max':float(data.max()),'finite':bool(np.isfinite(data).all())})
    manifest['assets'][name]={'width':N,'height':N,'tile_metres':tile,'physical_relief_metres':relief,
        'roughness_min':float(rough.min()),'roughness_max':float(rough.max()),'maps':rows,
        'linear_base_color_mean':color.mean(axis=(0,1)).tolist(),
        'boundary_height_step_x':float(np.mean(abs(height[:,0]-height[:,-1]))),
        'boundary_height_step_y':float(np.mean(abs(height[0]-height[-1]))),
        'interior_height_step_x':float(np.mean(abs(np.diff(height,axis=1)))),
        'interior_height_step_y':float(np.mean(abs(np.diff(height,axis=0))))}
    print('BAKED',name,flush=True)

# Author preview materials on a distinct in-memory copy of the kit.
bpy.ops.wm.open_mainfile(filepath=str(KIT))
scene=bpy.context.scene
for obj in bpy.data.objects:obj.hide_render=True;obj.hide_set(True)

def material(name,texture,tint=(1,1,1),tiling=1):
    mat=bpy.data.materials.new(name);mat.use_nodes=True
    nodes=mat.node_tree.nodes;links=mat.node_tree.links;bsdf=nodes.get('Principled BSDF')
    uv=nodes.new('ShaderNodeTexCoord');scale=nodes.new('ShaderNodeVectorMath');scale.operation='SCALE';scale.inputs[3].default_value=tiling
    links.new(uv.outputs['UV'],scale.inputs[0])
    for channel,target in [('Diffuse','Base Color'),('Roughness','Roughness'),('NormalGL',None)]:
        node=nodes.new('ShaderNodeTexImage');node.image=bpy.data.images.load(str(ROOT/(texture+'_'+channel+'.png')),check_existing=True)
        node.extension='REPEAT';node.interpolation='Linear';links.new(scale.outputs['Vector'],node.inputs['Vector'])
        if channel!='Diffuse':node.image.colorspace_settings.name='Non-Color'
        if target=='Base Color':
            mix=nodes.new('ShaderNodeMixRGB');mix.blend_type='MULTIPLY';mix.inputs[0].default_value=1;mix.inputs[2].default_value=(*tint,1)
            links.new(node.outputs['Color'],mix.inputs[1]);links.new(mix.outputs[0],bsdf.inputs[target])
        elif target:links.new(node.outputs['Color'],bsdf.inputs[target])
        else:
            normal=nodes.new('ShaderNodeNormalMap');normal.inputs['Strength'].default_value=.6 if texture=='WalnutContinuous' else .8
            links.new(node.outputs['Color'],normal.inputs['Color']);links.new(normal.outputs['Normal'],bsdf.inputs['Normal'])
    return mat

wood=material('Bake / continuous furniture walnut','WalnutContinuous')
linenMat=material('Bake / neutral linen','LinenPlain',tiling=2)
chairMat=material('Bake / green upholstery','LinenPlain',(.34,.45,.39),tiling=3)
for obj in bpy.data.objects:
    if obj.type!='MESH':continue
    for slot in obj.material_slots:
        if not slot.material:continue
        name=slot.material.name
        if name in ['OV_Walnut','OV_WalnutEdge']:slot.material=wood
        elif name=='OV_Linen':slot.material=linenMat
        elif name=='OV_ChairFabric':slot.material=chairMat

scene.render.engine='CYCLES';scene.cycles.samples=32;scene.cycles.use_denoising=True
scene.render.resolution_x=1280;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.world.color=(.07,.07,.07);scene.view_settings.view_transform='AgX'
bpy.ops.mesh.primitive_plane_add(size=100,location=(0,0,-.008));floor=bpy.context.object;floor.name='Preview floor';floor.hide_render=False
gray=bpy.data.materials.new('Preview neutral');gray.diffuse_color=(.12,.125,.12,1);floor.data.materials.append(gray)
for name,p,energy,size,color in [('Softbox',(-3,-4,4),450,3.5,(1,.92,.82)),('Fill',(3,-1,3),180,3,(.80,.89,1)),('Rim',(0,3,4),350,3,(1,.94,.86))]:
    data=bpy.data.lights.new(name,'AREA');data.energy=energy;data.size=size;data.color=color
    obj=bpy.data.objects.new(name,data);scene.collection.objects.link(obj);obj.location=p;obj.rotation_euler=(Vector((0,0,.7))-obj.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();camera=bpy.context.object;camera.name='Material preview camera';scene.camera=camera;camera.data.lens=56
for asset in ['Armchair','KeyConsole','ReceptionCounter','DressedWindow']:
    if os.environ.get('EP_MATERIAL_PREVIEW_ONLY') and asset not in os.environ['EP_MATERIAL_PREVIEW_ONLY'].split(','):continue
    root=bpy.data.objects.get('EP_OV_'+asset)
    if not root:continue
    objects=[root]+list(root.children_recursive)
    for obj in objects:obj.hide_render=False;obj.hide_set(False)
    verts=[obj.matrix_world@Vector(c) for obj in objects if obj.type=='MESH' for c in obj.bound_box]
    size=max(max(p[i] for p in verts)-min(p[i] for p in verts) for i in range(3));height=max(p.z for p in verts)
    center=Vector((0,0,height*.49));camera.location=center+Vector((-.60,-1,.51)).normalized()*max(size*2.9,2.0)
    camera.rotation_euler=(center-camera.location).to_track_quat('-Z','Y').to_euler()
    scene.render.filepath=str(ROOT/('Preview_'+asset+'.png'));bpy.ops.render.render(write_still=True)
    for obj in objects:obj.hide_render=True;obj.hide_set(True)
for image in bpy.data.images:
    if image.source=='FILE' and image.filepath.startswith(str(ROOT)):image.pack()
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'FurnitureMaterials.blend'))
manifest['source_kit_sha256_after']=hashlib.sha256(KIT.read_bytes()).hexdigest()
assert manifest['source_kit_sha256_before']==manifest['source_kit_sha256_after']
manifest['preview_status']='Blender neutral presentation, not Unity integration or Game-view verification.'
(ROOT/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print('MATERIAL BAKE COMPLETE; source kit unchanged',flush=True)
