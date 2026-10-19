"""Reproduce layout coordinates/projection only; standard Python, no Unity access.

Reads original project manifests; writes layout.json beside this script. This is
geometric previsualization, not navigation, lighting, audio or human validation.
"""
from pathlib import Path
import json
import math

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
manifest = json.loads((ROOT / 'ArtSource/Blender/IsolationControl/manifest.json').read_text())
anchors = manifest['assets']['IsolationPanel']['anchors_blender_xyz']


def add(a, b):
    return [a[i] + b[i] for i in range(3)]


def subtract(a, b):
    return [a[i] - b[i] for i in range(3)]


def length(a):
    return math.sqrt(sum(v * v for v in a))


def yaw_rotate(p, yaw):
    a = math.radians(yaw)
    return [p[0] * math.cos(a) + p[2] * math.sin(a), p[1],
            -p[0] * math.sin(a) + p[2] * math.cos(a)]


def source_to_world(p, origin, yaw):
    return add(origin, yaw_rotate([p[0], p[2], -p[1]], yaw))


def aim(eye, target):
    d = subtract(target, eye)
    return {'yaw': math.degrees(math.atan2(d[0], d[2])) % 360,
            'pitch': -math.degrees(math.atan2(d[1], math.hypot(d[0], d[2])))}


def project(eye, angles, p):
    y, a = math.radians(angles['yaw']), math.radians(angles['pitch'])
    right = [math.cos(y), 0, -math.sin(y)]
    up = [math.sin(y)*math.sin(a), math.cos(a), math.cos(y)*math.sin(a)]
    forward = [math.sin(y)*math.cos(a), -math.sin(a), math.cos(y)*math.cos(a)]
    d = subtract(p, eye)
    dot = lambda axis: sum(d[i] * axis[i] for i in range(3))
    depth = dot(forward)
    t = math.tan(math.radians(30))
    return {'viewport_x': .5 + .5 * dot(right) / (depth*t*16/9),
            'viewport_y': .5 + .5 * dot(up) / (depth*t),
            'depth_m': depth}


control = [5.75, 1.35, -35.9]
control_yaw = 285
eye = [4.40, 1.63, -35.45]
grips = {state: source_to_world(anchors['IsolationPanel__LEVER_GRIP_' + state], control, control_yaw)
         for state in ['MARCHA', 'AISLADO']}
angles = aim(eye, grips['MARCHA'])
pivot = anchors['IsolationPanel__LEVER_PIVOT']
offset = subtract(anchors['IsolationPanel__LEVER_GRIP_MARCHA'], pivot)
grip_samples = []
for step in range(71):
    a = math.radians(step)
    point = add(pivot, [offset[0]*math.cos(a)+offset[2]*math.sin(a), offset[1],
                       -offset[0]*math.sin(a)+offset[2]*math.cos(a)])
    grip_samples.append(source_to_world(point, control, control_yaw))

fan_centre = [6.174, .535, -36.5]  # Existing rear recess centre, unmodified pump.
fan_points = {'centre': fan_centre,
              'top': add(fan_centre, [0, .188, 0]),
              'bottom': add(fan_centre, [0, -.188, 0]),
              'north': add(fan_centre, [0, 0, .188]),
              'south': add(fan_centre, [0, 0, -.188])}
# Illustrative approach band, never a new gameplay acceptance zone. These
# projections do not simulate occlusion, lighting or human use of the camera.
approach_samples = []
for ix in range(6):
    for iz in range(6):
        sample_eye = [4.30 + ix*.10, 1.63, -35.50 + iz*.10]
        sample_angles = aim(sample_eye, grips['MARCHA'])
        projections = [project(sample_eye, sample_angles, p) for p in fan_points.values()]
        approach_samples.append({
            'eye': sample_eye,
            'maximum_sweep_reach_m': max(length(subtract(p, sample_eye)) for p in grip_samples),
            'fan_minimum_viewport_y': min(p['viewport_y'] for p in projections),
            'fan_minimum_viewport_x': min(p['viewport_x'] for p in projections),
            'fan_maximum_viewport_x': max(p['viewport_x'] for p in projections)})
route = [[0, .03, -29.6], [4.40, .03, -30.3], [4.40, .03, -35.45],
         [4.65, .03, -36.8], [4.65, .03, -44], [0, .03, -46], [0, .03, -49.5]]
baseline = [[0, .03, -29.6], [4.5, .03, -30.3], [4.5, .03, -36.5],
            [4.5, .03, -44], [0, .03, -46], [0, .03, -49.5]]
distance = lambda points: sum(length(subtract(b, a)) for a, b in zip(points, points[1:]))

data = {
    'status': 'PROPOSED; source-derived geometry only; no Unity queries or mutations',
    'coordinates': 'Unity world metres; body Y=.03; eye Y=1.63; FOV vertical60, 16:9',
    'preserve': {
        'receiver': [0, 0, -34.9], 'pump_A': [6.89, 0, -36.5], 'pump_B': [6.89, 0, -44],
        'collector_x_height': [7.3, 1.85], 'B_gauge': [5.918, 1.75, -44],
        'exit_door_plane_z': -48, 'entry_trigger': [0, 1.1, -29.2],
        'motor_source': [6.89, .55, -36.5], 'purge_source': [7.3, 1.85, -44],
        'exit_floor_fixture': [1.58, .38, -47.82], 'grip_reach': 1.65,
        'hold_s': 1.8, 'motor_coast_s': 2.2, 'drain_s': 3.0},
    'control_cluster': {
        'old_anchor': [5.9, 1.5, -36.5], 'new_anchor': control,
        'delta_world_yaw': 15, 'resulting_original_model_placement_yaw': control_yaw,
        'assembly_rule': 'Create parent at old anchor with identity rotation; parent top-level authored pieces preserving world pose; then move parent to new anchor and yaw+15. Preserve dynamic pivot children and all component refs.',
        'grips': grips,
        'pedestal_shortening_m': .15,
        'control_and_latch_sources': add(control, yaw_rotate([-.2, 0, 0], 15))},
    'cabinet': {'model_root': [2.75, 0, -39], 'collider_centre': [2.75, 1.35, -39],
                'collider_size': [1.7, 2.7, 6], 'yaw': 0,
                'world_x': [1.9, 3.6], 'world_z': [-42, -36]},
    'clearance': {'pump_collision_west_x': 6.115, 'raw_aisle_width_m': 2.515,
                  'character_radius_m': .32, 'skin_width_m': .03,
                  'straight_route_x': 4.65,
                  'capsule_surface_to_cabinet_m': 4.65-.32-3.6,
                  'capsule_surface_to_pump_m': 6.115-4.65-.32,
                  'note': 'Static envelope margins; existing circulation/physics must be inspected by root.'},
    'reserve': {
        'repurposed_first_fixture': {'enclosure': [3.69, 1.08, -36.15],
            'bracket_centre': [3.62, 1.08, -36.15], 'bracket_size': [.16, .045, .12],
            'light': [3.80, 1.08, -36.15], 'target': fan_centre,
            'type': 'Spot', 'intensity': 1.1, 'range': 4.3, 'outer_degrees': 90,
            'inner_degrees': 55, 'colour': [.70, .81, .71],
            'note': 'Reuse the same reserve Light reference; replace only first floor shoe/pedestal with attached bracket/hood. Target/intensity provisional until actual Unity comparison.'},
        'second_floor_fixture': [3.72, .38, -39.7],
        'third_floor_fixture': [3.72, .38, -43.9],
        'unchanged_exit_floor_fixture': [1.58, .38, -47.82]},
    'route': route,
    'geometric_length_m': distance(route), 'baseline_length_m': distance(baseline),
    'delta_distance_m': distance(route)-distance(baseline),
    'operator_camera': {'position': eye, **angles,
        'max_grip_distance_full_sweep_m': max(length(subtract(p, eye)) for p in grip_samples),
        'grip_distance_end_m': length(subtract(grips['AISLADO'], eye)),
        'fan_projection': {n: project(eye, angles, p) for n, p in fan_points.items()},
        'projection_limit': 'Analytic frustum projection; does not establish actual lighting, occlusion or readability.'},
    'illustrative_approach_band': {
        'x': [4.30, 4.80], 'z': [-35.50, -35.0],
        'grid_spacing_m': .10, 'samples': len(approach_samples),
        'max_sweep_reach_m': max(s['maximum_sweep_reach_m'] for s in approach_samples),
        'minimum_fan_viewport_y': min(s['fan_minimum_viewport_y'] for s in approach_samples),
        'minimum_fan_viewport_x': min(s['fan_minimum_viewport_x'] for s in approach_samples),
        'maximum_fan_viewport_x': max(s['fan_maximum_viewport_x'] for s in approach_samples),
        'constraint': 'No trigger, snap, camera modification or new interaction rule. Source-derived composition pending Unity camera/occlusion inspection.'},
    'additional_camera_suggestions': [
        {'name': 'entry oblique', 'position': [4.15,1.63,-32.7], 'yaw': 160, 'pitch': 3},
        {'name': 'walk beside machinery', 'position': [4.65,1.63,-40.2], 'yaw': 170, 'pitch': 6},
        {'name': 'exit approach', 'position': [4.65,1.63,-44.1], 'yaw': 230, 'pitch': 3},
        {'name': 'optional look back, never required', 'position': [0,1.63,-46.6], 'yaw': 65, 'pitch': 4}]
}
(HERE / 'layout.json').write_text(json.dumps(data, indent=2)+'\n')
print(json.dumps({'max_grip_m': data['operator_camera']['max_grip_distance_full_sweep_m'],
                  'fan_viewport': data['operator_camera']['fan_projection'],
                  'route_delta_m': data['delta_distance_m'],
                  'aisle_m': data['clearance']['raw_aisle_width_m']}, indent=2))
