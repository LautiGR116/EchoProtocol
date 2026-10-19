import importlib.util,json
from pathlib import Path
BASE=Path(__file__).resolve().parent
spec=importlib.util.spec_from_file_location('common',BASE.parent/'RecoveryOffice'/'validate_recovery_office.py');common=importlib.util.module_from_spec(spec);spec.loader.exec_module(common)
m=json.loads((BASE/'manifest.json').read_text());results=[]
for n in ['ReservePack','ReserveDock']:results.append(common.validate_asset(n,m['assets'][n],BASE))
p=m['assets']['ReservePack'];d=m['assets']['ReserveDock'];dock=d['anchors_blender_xyz']['ReserveDock__DOCKED_PACK_BOTTOM'];pack_bb=p['bounds_blender_xyz'];rail_clearance=.072-max(abs(x) for x in pack_bb[0]);seat_gap=pack_bb[2][0]+dock[2]-(-.1005);wall_clearance=-(pack_bb[1][1]+dock[1]);assembly={'side_rail_clearance_m':rail_clearance,'seat_gap_m':seat_gap,'rear_wall_clearance_m':wall_clearance,'pass':rail_clearance>=.002 and 0<=seat_gap<=.001 and wall_clearance>.02}
r={'pass':all(x['pass'] for x in results) and assembly['pass'],'assets':results,'assembly':assembly,'scope':'Independent FBX reimport; finite geometry/materials/UV/anchors/bounds and numerical dock fit. No gameplay or human visual acceptance.'};(BASE/'roundtrip-checks.json').write_text(json.dumps(r,indent=2)+'\n');print(json.dumps({'pass':r['pass'],'triangles':m['total_triangles'],'assembly':assembly}));assert r['pass']
