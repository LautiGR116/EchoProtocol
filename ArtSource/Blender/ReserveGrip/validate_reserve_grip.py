"""Independent disposable Blender FBX reimport, surface and grip checks."""
import importlib.util,json,math,hashlib
from pathlib import Path
import bpy
from mathutils import Vector
from mathutils.bvhtree import BVHTree
BASE=Path(__file__).resolve().parent
spec=importlib.util.spec_from_file_location('roundtrip_common',BASE.parent/'RecoveryOffice/validate_recovery_office.py')
common=importlib.util.module_from_spec(spec);spec.loader.exec_module(common)

def main():
 manifest=json.loads((BASE/'manifest.json').read_text());entry=manifest['assets']['ReserveGrip']
 result=common.validate_asset('ReserveGrip',entry,BASE)
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH'];byname={o.name:o for o in meshes}
 hand=byname['ReserveRightHand'];hand.data.calc_loop_triangles()
 points=[hand.matrix_world@v.co for v in hand.data.vertices]
 edge_count={}
 adjacency={i:set() for i in range(len(hand.data.vertices))}
 for e in hand.data.edges:
  a,b=e.vertices;adjacency[a].add(b);adjacency[b].add(a)
 for poly in hand.data.polygons:
  for edge in poly.edge_keys:edge_count[tuple(sorted(edge))]=edge_count.get(tuple(sorted(edge)),0)+1
 unseen=set(adjacency);components=0
 while unseen:
  components+=1;todo=[unseen.pop()]
  while todo:
   for other in adjacency[todo.pop()]:
    if other in unseen:unseen.remove(other);todo.append(other)
 samples=points[:]
 for tri in hand.data.loop_triangles:
  samples.append(sum((points[i] for i in tri.vertices),Vector())/3)
 nearbar=[p for p in samples if abs(p.x)<.034 and abs(p.y)<.04 and abs(p.z-.201)<.045]
 radial=[math.hypot(p.y,p.z-.201) for p in nearbar]
 inside=sum(r<.0095 for r in radial)
 tree=BVHTree.FromObject(hand,bpy.context.evaluated_depsgraph_get())
 nail=byname['ReserveThumbNail'];nail_distances=[tree.find_nearest(hand.matrix_world.inverted() @ (nail.matrix_world @ v.co))[3] for v in nail.data.vertices]
 nail_close=min(nail_distances)<.001 and max(nail_distances)<.006
 checks={'skin_connected_components':components,'skin_is_one_closed_surface':components==1 and all(c==2 for c in edge_count.values()),
  'armatures':sum(o.type=='ARMATURE' for o in bpy.context.scene.objects),
  'animation_data_present':any(o.animation_data and o.animation_data.action for o in bpy.context.scene.objects),
  'all_mesh_pivots_at_common_origin':all(common.vector_error(o.matrix_world.translation,(0,0,0))<.0001 for o in meshes),
  'handle_axis_sample_count':len(radial),'handle_min_skin_radius_m':min(radial) if radial else None,
  'skin_samples_deeper_than_half_mm_in_handle':inside,
  'skin_min_z_m':min(p.z for p in points),
  'sleeve_exit_has_negative_source_x_and_y':entry['anchors_blender_xyz']['ReserveGrip__SLEEVE_EXIT'][0]<-.1 and entry['anchors_blender_xyz']['ReserveGrip__SLEEVE_EXIT'][1]<-.1,
  'nail_min_distance_to_skin_m':min(nail_distances),'nail_max_distance_to_skin_m':max(nail_distances),'nail_remains_in_contact_with_skin':nail_close,
  'no_pack_mesh_exported':set(byname)=={'ReserveRightHand','ReserveJacketSleeve','ReserveThumbNail'}}
 failures=result['failures']
 if not checks['skin_is_one_closed_surface']:failures.append('hand is not one closed continuous surface')
 if checks['armatures'] or checks['animation_data_present']:failures.append('unexpected armature or animation')
 if not checks['all_mesh_pivots_at_common_origin']:failures.append('mesh root differs from pack common root')
 if not nail_close:failures.append('nail does not remain in contact with the thumb surface')
 if inside:failures.append('hand penetrates grip cylinder more than .5mm in sampled surface')
 if not checks['sleeve_exit_has_negative_source_x_and_y'] or not checks['no_pack_mesh_exported']:failures.append('asset content/placement contract failed')
 if result['triangles']>8000:failures.append('triangle budget exceeds8000')
 result['pass']=not failures;result['checks'].update(checks)
 report={'all_pass':result['pass'],'scope':'Blender FBX roundtrip only: finite vertices/UV/materials, bounds/anchors/pivots, 8000triangle budget, one closed skin surface, no armature/animation/pack geometry, and sampled clearance around the supplied .010m-radius handle. This does not certify anatomical realism, every mesh intersection, Unity view/clipping, shadow or interaction settings, or human visual approval.','total_triangles':result['triangles'],'result':result}
 (BASE/'roundtrip-checks.json').write_text(json.dumps(common.json_safe(report),indent=2,allow_nan=False)+'\n')
 print('PASS' if result['pass'] else 'FAIL',result['triangles'],checks,failures,flush=True)
 if not result['pass']:raise RuntimeError('ReserveGrip check failed')
if __name__=='__main__':main()
