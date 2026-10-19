using EchoProtocol.Echo;
using EchoProtocol.Horror;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    public enum CampaignChapter { Protocol, Descent, Pressure, House, Gallery, Archive, Departure, Complete }

    // Persistent authored consequences are separate from immutable Echo samples.
    public sealed class CampaignDirector : MonoBehaviour
    {
        [SerializeField] private FirstPersonController player;
        [SerializeField] private Camera view;
        [SerializeField] private LoopController loop;
        [SerializeField] private EchoRecorder recorder;
        [SerializeField] private GameSession session;
        [SerializeField] private VS1DenialEvidence evidence;
        [SerializeField] private SD1SubtitleDisplay thought;
        [SerializeField] private Transform[] origins;
        [SerializeField] private SlidingDoor[] doors;
        [SerializeField] private BoxCollider pressureZone;
        [SerializeField] private BoxCollider[] echoBays;
        [SerializeField] private BoxCollider[] playerBays;
        [SerializeField] private BoxCollider archiveEchoZone;
        [SerializeField] private BoxCollider departureZone;
        [SerializeField] private CampaignStation[] stations;
        [SerializeField] private HouseMemory house;
        [SerializeField] private PressureIsolation pressureIsolation;
        [SerializeField] private MachineCoverEncounter machineCover;
        [SerializeField] private FinalDoorMemory finalDoor;
        [SerializeField] private VS1HubReturnGate returnWithoutReview;
        [Tooltip("Threshold only: a volume on the original Hub side of the completed R3 arrival. Never the distant P3 exit.")]
        [SerializeField] private BoxCollider returnArrivalZone;
        [Tooltip("Optional local SUBSUELO sign/pilot. Revealed once at the actual Hub arrival, hidden when Descent starts.")]
        [SerializeField] private GameObject returnDescentGuide;
        [SerializeField] private ReturnRecordingBoundary returnRecordingBoundary;
        [Tooltip("Optional continuation binding: later chapters retain the completed first-act lighting without replaying its event.")]
        [SerializeField] private VS1StrongEventOne firstActLighting;
        [SerializeField] private string checkpointKey = "EP.CampaignCheckpoint.v1";
        [SerializeField] private EchoProtocol.Audio.PressureWorkAudio pressureWorkAudio;
        [SerializeField] private Light[] pressureLights;
        [SerializeField] private Light[] homeLights;
        [SerializeField] private Light[] galleryLights;
        [SerializeField] private GameObject homePresence;
        [SerializeField] private Transform homeChair;
        [SerializeField] private AudioSource machinery;
        [SerializeField] private AudioSource[] cues;
        [SerializeField] private AudioClip[] sounds;
        private CampaignStation working;
        private float work, pressure, coincident, finalWitness, departureTime;
        private bool pressurePremonition;
        private bool valveA,valveB,phone,homeChanged,departureCue,availableAnnounced;
        private int documents,circuit,companion;
        private float nextPulse, ringAt, stageStarted, presenceSeen;
        private string status="";
        public bool UsesPressureIsolation=>pressureIsolation || machineCover;
        public MachineCoverEncounter CoverEncounter=>machineCover;
        public PressureIsolation IsolationControl=>pressureIsolation;
        public bool IsReturnRecordingSuspended=>returnRecordingBoundary&&returnRecordingBoundary.IsSuspended&&(!pressureIsolation||Chapter<CampaignChapter.Pressure);
        public string StatusText=>machineCover&&Chapter==CampaignChapter.Pressure?machineCover.StatusText:IsReturnRecordingSuspended?returnRecordingBoundary.StatusText:pressureIsolation&&Chapter==CampaignChapter.Pressure?pressureIsolation.StatusText:status;
        public float WorkFraction=>working?Mathf.Clamp01(work/(Chapter==CampaignChapter.Pressure?6f:4f)):0;
        private GUIStyle objectiveStyle,statusStyle;
        public CampaignChapter Chapter {get;private set;}
        public float Pressure=>machineCover?machineCover.Pressure01:pressureIsolation?pressureIsolation.Pressure01:pressure;
        public bool PressureSubjectAligned=>Chapter==CampaignChapter.Pressure&&(EchoIn(pressureZone,270)||(PlayerIn(pressureZone)&&Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y,270))<35f));
        public int ValvesDone=>(valveA?1:0)+(valveB?1:0);
        public int DocumentsRead=>documents;
        public float DepartureFade => !session || session.IsPaused || HideHudForPresence || Chapter==CampaignChapter.Protocol || finalDoor&&finalDoor.Active ? 0 : departureTime>0 ? Mathf.Clamp01((departureTime-1)/3) : 0;
        public int CircuitsRestored=>circuit;
        public int GalleryPairs=>companion;
        public bool HasFinalWitness=>finalWitness>=2f;
        private string objective="";
        public string Objective {get=>machineCover&&Chapter==CampaignChapter.Pressure?machineCover.Objective:IsReturnRecordingSuspended?returnRecordingBoundary.Objective:pressureIsolation&&Chapter==CampaignChapter.Pressure?pressureIsolation.Objective:objective;private set=>objective=value;}
        public bool HasHomeChanged=>homeChanged;
        public bool HideHudForPresence=>homePresence&&homePresence.activeSelf&&presenceSeen>.05f;
        public bool PresenceResolved=>homeChanged&&presenceSeen>.7f&&!homePresence.activeSelf;
        public bool IsWorking=>working;
        public CampaignStation WorkingStation=>working;
        public float WorkProgress=>work;
        public bool SuppressSaves {get;set;}
        private string SaveKey=>checkpointKey;
        private bool ReviewAvailable=>returnWithoutReview ? returnWithoutReview.IsOpen : evidence.HasWitnessed;
        private bool AtReviewAnnouncement=>!returnWithoutReview || (player.isActiveAndEnabled && returnArrivalZone &&
            returnArrivalZone.enabled && returnArrivalZone.gameObject.activeInHierarchy &&
            new Bounds(returnArrivalZone.center,returnArrivalZone.size).Contains(
                returnArrivalZone.transform.InverseTransformPoint(player.transform.position+Vector3.up)));
        public bool HasSavedCheckpoint {get {int saved=PlayerPrefs.GetInt(SaveKey,0);return saved>=2&&saved<=5&&(!(pressureIsolation||machineCover)||saved!=4);}}
        public string SavedChapterTitle=>machineCover&&PlayerPrefs.GetInt(SaveKey,0)==2?"La máquina que te cubre":pressureIsolation&&PlayerPrefs.GetInt(SaveKey,0)==2?"Subsuelo / aislamiento":finalDoor&&PlayerPrefs.GetInt(SaveKey,0)==5?"Del otro lado":new[]{"","","Presión","La casa","Acompañamiento","Archivo"}[Mathf.Clamp(PlayerPrefs.GetInt(SaveKey,0),0,5)];
        private void SaveCheckpoint()
        {
            if(SuppressSaves)return;
            PlayerPrefs.SetInt(SaveKey,(int)Chapter);
            if(pressureIsolation)PlayerPrefs.SetInt(SaveKey+".isolated",pressureIsolation.IsLatched?1:0);
            if(machineCover)PlayerPrefs.SetInt(SaveKey+".covered",machineCover.Complete?1:0);
            if(finalDoor&&Chapter==CampaignChapter.Archive)
            {
                PlayerPrefs.SetInt(SaveKey+".answered",house.HasAnswered?1:0);
                PlayerPrefs.SetInt(SaveKey+".light",(int)house.PowerLocation);
            }
            PlayerPrefs.Save();
        }
        public void ClearCheckpointSave(){PlayerPrefs.DeleteKey(SaveKey);PlayerPrefs.DeleteKey(SaveKey+".answered");PlayerPrefs.DeleteKey(SaveKey+".light");PlayerPrefs.DeleteKey(SaveKey+".isolated");PlayerPrefs.DeleteKey(SaveKey+".covered");PlayerPrefs.Save();}
        public bool RestoreCheckpoint()
        {
            if(!HasSavedCheckpoint)return false;
            if(finalDoor && finalDoor.Active)return false;
            var saved=(CampaignChapter)PlayerPrefs.GetInt(SaveKey);int index=(int)saved-2;
            if(firstActLighting && !firstActLighting.RestoreCompletedLighting())return false;
            evidence.enabled=false;doors[0].SetOpen(true);
            if(machineCover)
            {
                machineCover.RestoreState(saved>=CampaignChapter.House||PlayerPrefs.GetInt(SaveKey+".covered",0)==1);
                if(saved==CampaignChapter.Pressure)
                {
                    player.ResetPose(machineCover.Origin.position,machineCover.Origin.rotation);
                    Change(CampaignChapter.Pressure,machineCover.Objective);return true;
                }
            }
            if(pressureIsolation)
            {
                bool isolated=saved>=CampaignChapter.House||PlayerPrefs.GetInt(SaveKey+".isolated",0)==1;
                if(!pressureIsolation.RestoreState(isolated))return false;
                if(saved==CampaignChapter.Pressure)
                {
                    var safeOrigin=pressureIsolation.ContinuationOrigin;
                    player.ResetPose(safeOrigin.position,safeOrigin.rotation);
                    Change(CampaignChapter.Pressure,pressureIsolation.Objective);
                    return true;
                }
            }
            if(saved>=CampaignChapter.House){valveA=valveB=true;doors[1].SetOpen(true);if(!pressureIsolation)machinery.Stop();}
            if(finalDoor&&saved==CampaignChapter.Archive)
            {
                house.RestoreAtReview(PlayerPrefs.GetInt(SaveKey+".answered",0)==1,(HousePowerLocation)Mathf.Clamp(PlayerPrefs.GetInt(SaveKey+".light",0),0,3));
                player.ResetPose(origins[3].position,origins[3].rotation);
                Change(CampaignChapter.Archive,finalDoor.Objective);
                if(!finalDoor.EnterReview())return false;
                Objective=finalDoor.Objective;
                thought.Show("Volviste al origen. El registro está vacío.",4);return true;
            }
            if(saved>=CampaignChapter.Gallery){documents=7;circuit=3;phone=true;if(!homeChanged)homeChair.position+=new Vector3(1.1f,0,.6f);homeChanged=true;doors[2].SetOpen(true);}
            if(saved>=CampaignChapter.Archive){companion=3;for(int i=3;i<6;i++)doors[i].SetOpen(true);foreach(var l in galleryLights)l.enabled=false;}
            player.ResetPose(origins[index].position,origins[index].rotation);if(!pressureIsolation)Origin(index);
            string[] objectives={"PRESIÓN · Registrá 30 s en la cámara azul. RECALL. Cebá las dos válvulas.","LA CASA / LECTURA · Atendé el teléfono. Podés revisar los documentos de la casa.","ACOMPAÑAMIENTO · Registrá pausas de 8 s en I, II y III. RECALL; acompañá por el otro lado.","ARCHIVO · Registrate frente al vidrio. RECALL y observá desde la marca blanca."};
            Change(saved,objectives[index]);
            if(saved==CampaignChapter.House){loop.enabled=false;recorder.CancelRecording();ringAt=Time.time+4;if(house){house.EnterHouse();Objective=house.Objective;}}
            thought.Show("La sesión continúa desde el comienzo de este registro.",4);return true;
        }
        private void Awake()
        {
            if(!player||!view||!loop||!recorder||!session||!evidence||!thought||origins.Length!=4||doors.Length!=7||!pressureZone||echoBays.Length!=3||playerBays.Length!=3||!archiveEchoZone||!departureZone||!homePresence||!homeChair||!machinery||cues.Length<4||sounds.Length<6)
            {Debug.LogError("CampaignDirector: incomplete authored scene references.",this);enabled=false;return;}
            homePresence.SetActive(false);
            if(returnDescentGuide)returnDescentGuide.SetActive(false);
        }
#if UNITY_EDITOR
        // Isolated chapter tests explicitly skip act one; full-route tests never call this.
        public void BeginExpansionProbe()
        { evidence.enabled=false; doors[0].SetOpen(true); Change(CampaignChapter.Descent,"ISOLATED EXPANSION TEST"); }
        public void PrepareHouseProbe()
        {evidence.enabled=false;valveA=valveB=true;doors[0].SetOpen(true);doors[1].SetOpen(true);Change(CampaignChapter.Pressure,"ISOLATED HOUSE TEST");if(pressureIsolation)pressureIsolation.RestoreState(true);}
        public void PrepareGalleryProbe()
        {PrepareHouseProbe();documents=7;circuit=3;doors[2].SetOpen(true);Change(CampaignChapter.House,"ISOLATED GALLERY TEST");}
        public void PrepareArchiveProbe()
        {PrepareGalleryProbe();companion=3;for(int i=3;i<6;i++)doors[i].SetOpen(true);Change(CampaignChapter.Gallery,"ISOLATED ARCHIVE TEST");}
#endif
        // Only the opt-in physical entry volume calls this. No new recording origin.
        public bool BeginPressureIsolation()
        {
            if(!pressureIsolation||Chapter!=CampaignChapter.Descent||session.IsPaused||!player.isActiveAndEnabled)return false;
            if(!pressureIsolation.EnterService())return false;
            Change(CampaignChapter.Pressure,pressureIsolation.Objective);
            SaveCheckpoint();
            return true;
        }
        public void CommitPressureIsolation()
        {
            if(!pressureIsolation||!pressureIsolation.IsLatched||Chapter!=CampaignChapter.Pressure||session.IsPaused)return;
            SaveCheckpoint();
        }
        private void Change(CampaignChapter chapter,string objective)
        {Chapter=chapter;Objective=objective;status="";stageStarted=Time.time;working=null;work=0;if(chapter!=CampaignChapter.Protocol&&returnDescentGuide)returnDescentGuide.SetActive(false);}
        private void Origin(int index)
        {loop.enabled=true;loop.SetRecallCheckpoint(origins[index]);}
        public bool BeginMachineCover()
        {
            if(!machineCover||Chapter!=CampaignChapter.Descent||!machineCover.Enter())return false;
            Change(CampaignChapter.Pressure,machineCover.Objective);SaveCheckpoint();return true;
        }
        public void CommitMachineCover()
        {if(machineCover&&machineCover.Complete&&Chapter==CampaignChapter.Pressure)SaveCheckpoint();}
        public bool BeginCoverPreview()
        {
            if(!machineCover||session.IsPaused||!player.isActiveAndEnabled)return false;
            if(firstActLighting&&!firstActLighting.RestoreCompletedLighting())return false;
            evidence.enabled=false;doors[0].SetOpenImmediately(true);
            Change(CampaignChapter.Descent,"");
            player.ResetPose(machineCover.Origin.position,machineCover.Origin.rotation);
            return BeginMachineCover();
        }
        private bool EchoIn(BoxCollider zone,float facing=-1)
        {
            var echo=loop.ActiveEcho;
            return echo && !echo.IsComplete && zone.bounds.Contains(echo.transform.position+Vector3.up)
                && (facing<0 || Mathf.Abs(Mathf.DeltaAngle(echo.transform.eulerAngles.y,facing))<35f);
        }
        private bool PlayerIn(BoxCollider zone)=>zone.bounds.Contains(player.transform.position+Vector3.up);
        public bool CanUse(CampaignStation station,GameObject who)
        {
            if(!enabled||session.IsPaused||who!=player.gameObject)return false;
            switch(station.Kind)
            {
                case StationKind.IntroLetter:return Chapter==CampaignChapter.Protocol;
                case StationKind.Descent:return Chapter==CampaignChapter.Protocol&&ReviewAvailable&&!thought.IsShowing;
                case StationKind.PressureOrigin:return !pressureIsolation&&!machineCover&&Chapter==CampaignChapter.Descent;
                case StationKind.ValveA:return !pressureIsolation&&!machineCover&&Chapter==CampaignChapter.Pressure&&!valveA;
                case StationKind.ValveB:return !pressureIsolation&&!machineCover&&Chapter==CampaignChapter.Pressure&&!valveB;
                case StationKind.HouseOrigin:return Chapter==CampaignChapter.Pressure&&(machineCover?machineCover.Complete:pressureIsolation?pressureIsolation.IsDrained:valveA&&valveB);
                case StationKind.NoteArrival:case StationKind.NoteWorkOrder:case StationKind.NoteConsent:return Chapter==CampaignChapter.House;
                case StationKind.Telephone:return !house&&Chapter==CampaignChapter.House&&!phone;
                case StationKind.CircuitKitchen:case StationKind.CircuitBedroom:case StationKind.CircuitHall:return !house&&Chapter==CampaignChapter.House&&phone&&circuit<3;
                case StationKind.GalleryOrigin:return Chapter==CampaignChapter.House&&(house?house.HasExited:circuit==3);
                case StationKind.ArchiveOrigin:return Chapter==CampaignChapter.Gallery&&companion==3;
                case StationKind.ArchiveLetter:return Chapter==CampaignChapter.Archive;
                case StationKind.Preserve:return Chapter==CampaignChapter.Archive&&HasFinalWitness;
                default:return false;
            }
        }
        public void Use(CampaignStation station)
        {
            if(!CanUse(station,player.gameObject))return;
            switch(station.Kind)
            {
                case StationKind.IntroLetter:
                    session.ShowDocument("La llave de 6B","A.,\n\nLa dejé en portería. Dijiste que no habías vuelto, pero te quedaste con ella toda la semana.\n\nNo necesito otra copia del recorrido. Necesito que me digas por qué esperaste del otro lado.\n\nSi vas a entrar otra vez, dejá encendida la luz del recibidor.\n\nMara");break;
                case StationKind.Descent:
                    evidence.enabled=false;doors[0].SetOpen(true);
                    Change(CampaignChapter.Descent,"BAJAR AL SUBSUELO · La sesión continúa");
                    station.SetCaption("REVISIÓN COMPLETA\nContinuar al subsuelo");
                    Cue(0,0,station.transform.position,.3f);break;
                case StationKind.PressureOrigin:
                    Origin(0);Change(CampaignChapter.Pressure,"PRESIÓN · Registrá 30 s en la cámara azul. RECALL. Cebá las dos válvulas.");
                    SaveCheckpoint();station.SetCaption("ORIGEN / PRESIÓN\n[R] vuelve aquí");
                    break;
                case StationKind.ValveA:case StationKind.ValveB:
                    if(pressure<.9f){thought.Show("Falta presión. Mi Echo tiene que permanecer en la cámara azul.",4);return;}
                    working=station;work=0;Cue(0,0,station.transform.position,.3f);break;
                case StationKind.HouseOrigin:
                    if(!pressureIsolation)Origin(1);loop.enabled=false;recorder.CancelRecording();if(!pressureIsolation)machinery.Stop();
                    Change(CampaignChapter.House,"LA CASA / LECTURA · Atendé el teléfono. Podés revisar los documentos de la casa.");
                    station.SetCaption("RECONSTRUCCIÓN / LECTURA\nRegistro en pausa");
                    SaveCheckpoint();if(house){house.EnterHouse();Objective=house.Objective;}else{thought.Show("El teléfono estaba junto a la ventana.",4);ringAt=Time.time+4;}break;
                case StationKind.NoteArrival:
                    documents|=1;session.ShowDocument("03:17 / Llegada","Hoja de recepción. Firma: A. Vera.\n\n«Regresé solo. Nadie me esperaba. No me detuve frente a esa puerta.»\n\nEl ingreso se produjo a las 03:17. La declaración fue escrita a las 08:40, después de la primera sesión.\n\nPortería anotó que Mara dejó una llave de 6B esa misma madrugada. No consta que haya entrado. La casilla de devolución sigue vacía.");break;
                case StationKind.NoteWorkOrder:
                    documents|=2;session.ShowDocument("La casa sin corriente","Orden de mantenimiento / vivienda 6B\n\nLa línea vuelve desde el fondo: COCINA → DORMITORIO → RECIBIDOR. Cada tablero necesita una comprobación sostenida. Si se interrumpe, repetir ese tablero.\n\n«No puentear el circuito del teléfono. El timbre siguió funcionando con la vivienda desconectada.»\n\nVisita pendiente. No retirar la llave de portería.");break;
                case StationKind.NoteConsent:
                    documents|=4;session.ShowDocument("Consentimiento / última sesión","A. Vera / solicitud de revisión\n\nEl protocolo conserva posición y orientación corporal. No conserva pensamientos, voces ni imágenes de lo que el sujeto miró.\n\n«Mara me preguntó por qué no abrí. Le dije que ya me había ido. No quiero demostrar que no estuve. Quiero saber por qué recuerdo haberme ido.»\n\nSolicitud manuscrita: mantener el registro original incluso si el solicitante pide borrarlo durante la revisión.");break;
                case StationKind.Telephone:
                    phone=true;station.SetCaption("SIN LÍNEA");foreach(var l in homeLights)l.enabled=false;
                    Change(CampaignChapter.House,"LA CASA · Restituí la línea: cocina → dormitorio → recibidor.");
                    Cue(1,2,station.transform.position,.23f);nextPulse=Time.time+5;break;
                case StationKind.CircuitKitchen:case StationKind.CircuitBedroom:case StationKind.CircuitHall:
                    int selected=(int)station.Kind-(int)StationKind.CircuitKitchen;
                    if(selected!=circuit){thought.Show("La línea llega desde el fondo: cocina, dormitorio, recibidor.",5);Cue(0,0,station.transform.position,.2f);return;}
                    working=station;work=0;Cue(0,0,station.transform.position,.25f);break;
                case StationKind.GalleryOrigin:
                    if(house)house.LeaveHouse();
                    Origin(2);Change(CampaignChapter.Gallery,"ACOMPAÑAMIENTO · Registrá una pausa de 8 s en I, II y III. RECALL; acompañá desde el otro lado.");
                    SaveCheckpoint();break;
                case StationKind.ArchiveOrigin:
                    Origin(3);Change(CampaignChapter.Archive,"ARCHIVO · Registrate frente al vidrio. RECALL y observá desde la marca blanca.");
                    SaveCheckpoint();break;
                case StationKind.ArchiveLetter:
                    session.ShowDocument("Una carta fuera del registro","A.,\n\nNo te pedí que recordaras mis palabras exactas. Te pregunté por qué esperaste del otro lado. Yo también puedo estar recordándolo mal.\n\nDejé la llave con el portero. No hace falta que vuelvas a abrir la puerta para contestarme.\n\nMara\n\n[Documento aportado por el solicitante. No forma parte de la grabación del Echo.]");break;
                case StationKind.Preserve:
                    loop.enabled=false;recorder.CancelRecording();doors[6].SetOpen(true);
                    Change(CampaignChapter.Departure,"REGISTRO CONSERVADO · Atravesá la última puerta.");
                    station.SetCaption("ORIGINAL CONSERVADO");
                    thought.Show("Que quede así.",4);Cue(3,5,station.transform.position,.25f);break;
            }
        }
        private void Update()
        {
            if(session.IsPaused)return;
            if(finalDoor&&finalDoor.Active)
            {
                if(Chapter!=CampaignChapter.Archive){Change(CampaignChapter.Archive,finalDoor.Objective);SaveCheckpoint();}
                Objective=finalDoor.Objective;status=finalDoor.StatusText;
                return;
            }
            if(Chapter==CampaignChapter.Protocol)
            {
                if(ReviewAvailable&&AtReviewAnnouncement&&!thought.IsShowing&&!availableAnnounced)
                {
                    availableAnnounced=true;
                    Station(StationKind.Descent).SetCaption("REGISTRO 02 / SUBSUELO\n[E] Habilitar paso");
                    if(returnWithoutReview&&returnDescentGuide)returnDescentGuide.SetActive(true);
                    thought.Show(returnWithoutReview?"Subsuelo. El terminal está junto a la salida.":"Se abrió otro registro. El terminal está en el vestíbulo.",5);
                }
                return;
            }
            if(Chapter==CampaignChapter.Pressure)
            {
                if(machineCover)return;
                if(pressureIsolation)return; // Its physical control owns pressure, lamps and sound.
                UpdatePressure();
            }
            if(Chapter==CampaignChapter.House)UpdateHouse();
            if(Chapter==CampaignChapter.Gallery)UpdateGallery();
            if(Chapter==CampaignChapter.Archive)
            {
                bool looking=Vector3.Dot(view.transform.forward,(archiveEchoZone.bounds.center-view.transform.position).normalized)>.92f;
                bool observing=Vector3.Distance(player.transform.position,new Vector3(2.5f,.03f,-115))<2;
                if(EchoIn(archiveEchoZone,90)&&looking&&observing)finalWitness+=Time.deltaTime;
                if(HasFinalWitness){Objective="ARCHIVO · El recorrido coincide. Conservá el original en el terminal.";Station(StationKind.Preserve).SetCaption("TOMA ACTUAL VERIFICADA\n[E] Conservar");}
            }
            if(Chapter==CampaignChapter.Departure)
            {
                if(!departureCue&&player.transform.position.z < -125)
                {departureCue=true;Cue(2,4,new Vector3(0,1,-122),.2f);}
                if(departureTime>0 || PlayerIn(departureZone)){if(departureTime==0)player.enabled=false;departureTime+=Time.deltaTime;if(departureTime>5){Chapter=CampaignChapter.Complete;session.Finish();}}
            }
            if(working)UpdateWork();
        }
        private void UpdatePressure()
        {
            if(!pressurePremonition&&PlayerIn(pressureZone)&&recorder.ElapsedTime>12&&!loop.ActiveEcho)
            {pressurePremonition=true;Cue(2,3,new Vector3(6.8f,1.4f,-41),.22f);}
            bool held=EchoIn(pressureZone,270);
            pressure=Mathf.MoveTowards(pressure,held?1:0,Time.deltaTime*(held?.5f:1.5f));
            machinery.volume=Mathf.Lerp(.24f,.055f,ValvesDone/2f);
            status=valveA&&valveB?"CÁMARA DRENADA / puerta habilitada":$"PRESIÓN {Mathf.RoundToInt(pressure*100)}%   ·   VÁLVULAS {ValvesDone}/2";
            if(valveA&&valveB){doors[1].SetOpen(true);Objective="LA CASA · Cruzá la compuerta. Activá la estación de entrada.";}
        }
        private void UpdateWork()
        {
            bool near=Vector3.Distance(player.transform.position,working.transform.position)<2.9f;
            bool looking=Vector3.Dot(view.transform.forward,(working.transform.position-view.transform.position).normalized)>.82f;
            bool hasPower=Chapter!=CampaignChapter.Pressure || pressure>.8f;
            if(!near||!looking||!hasPower)
            {working=null;work=0;status="";thought.Show(hasPower?"Se interrumpió la comprobación. Volvé a iniciarla mirando el tablero.":"Cayó la presión. Necesito una toma más larga.",4);return;}
            work+=Time.deltaTime;
            float duration=Chapter==CampaignChapter.Pressure?6f:4f;
            status=$"MANTENÉ LA MIRADA   {work:0.0} / {duration:0} s";
            if(work<duration)return;
            Cue(0,0,working.transform.position,.3f);
            if(Chapter==CampaignChapter.Pressure)
            {
                if(working.Kind==StationKind.ValveA)valveA=true;else valveB=true;
                working.SetCaption("VÁLVULA CEBADA\nPresión retenida");
                int index=ValvesDone-1;if(index<pressureLights.Length)pressureLights[index].enabled=false;
                if(!pressureWorkAudio)Cue(2,3,player.transform.position-player.transform.forward*2+Vector3.up,.22f);
                
            }
            else
            {
                homeLights[circuit].enabled=true;working.SetCaption("LÍNEA ESTABLE");circuit++;
                if(circuit==2){homeChair.position+=new Vector3(1.1f,0,.6f);homePresence.SetActive(true);homeChanged=true;Cue(2,3,new Vector3(0,1.2f,-54),.25f);}
                if(circuit==3){doors[2].SetOpen(true);Objective="ACOMPAÑAMIENTO · Salí por el fondo y activá el nuevo origen.";}
                nextPulse=Time.time+4;
            }
            working=null;work=0;status="";
        }
        private void UpdateHouse()
        {
            if(house){Objective=house.Objective;status=house.StatusText;return;}
            if(!phone&&Time.time>=ringAt){Cue(1,1,Station(StationKind.Telephone).transform.position,.22f);ringAt=Time.time+8;}
            if(phone&&circuit<3&&Time.time>=nextPulse)
            {var s=Station((StationKind)((int)StationKind.CircuitKitchen+circuit));Cue(1,2,s.transform.position,.18f);nextPulse=Time.time+6;}
            if(circuit==3){doors[2].SetOpen(true);Objective="ACOMPAÑAMIENTO · Salí por el fondo y activá el nuevo origen.";}
            if(homePresence.activeSelf && player.transform.position.z> -57)
            {
                Vector3 target=homePresence.transform.position+Vector3.up*1.5f;
                bool unobstructed=!Physics.Linecast(view.transform.position,target,out RaycastHit hit,~0,QueryTriggerInteraction.Ignore)||hit.transform.IsChildOf(homePresence.transform);
                bool seeing=unobstructed&&Vector3.Dot(view.transform.forward,(target-view.transform.position).normalized)>.90f;
                if(seeing)presenceSeen+=Time.deltaTime;
                // Let the figure be rendered and recognized; an immediate look-triggered hide would never be seen.
                if(presenceSeen>.7f && (!seeing || presenceSeen>1.6f))
                {homePresence.SetActive(false);Cue(2,4,player.transform.position-player.transform.forward*1.4f+Vector3.up,.21f);}
            }
        }
        private void UpdateGallery()
        {
            status=$"COINCIDENCIAS {companion}/3 · Las pausas grabadas no cambian";
            if(companion>=3){Objective="ARCHIVO · Cruzá al fondo. La última estación está encendida.";return;}
            bool paired=EchoIn(echoBays[companion],90)&&PlayerIn(playerBays[companion]);
            coincident=paired?coincident+Time.deltaTime:0;
            if(coincident<1.5f)return;
            doors[3+companion].SetOpen(true);
            galleryLights[companion].enabled=false;
            Cue(2,0,doors[3+companion].transform.position+Vector3.up,.24f);
            companion++;coincident=0;
            
        }
        private CampaignStation Station(StationKind kind)
        {foreach(var s in stations)if(s.Kind==kind)return s;throw new System.InvalidOperationException("Missing station "+kind);}
        private void Cue(int source,int sound,Vector3 position,float volume)
        {var a=cues[source];a.transform.position=position;a.clip=sounds[sound];a.volume=volume;a.Play();}
        private void OnGUI()
        {
            if(EchoProtocol.Presentation.FolioUI.Current)return;
            if(session==null||session.IsPaused||HideHudForPresence||Chapter==CampaignChapter.Protocol||finalDoor&&finalDoor.Active)return;
            if(objectiveStyle==null){objectiveStyle=new GUIStyle(GUI.skin.label){fontSize=17,wordWrap=true,alignment=TextAnchor.UpperRight};objectiveStyle.normal.textColor=new Color(.73f,.82f,.8f);statusStyle=new GUIStyle(objectiveStyle){fontSize=15};statusStyle.normal.textColor=new Color(.4f,.85f,.8f);}
            GUI.depth=-1000;float width=Mathf.Min(590,Screen.width*.48f);
            if(Time.time-stageStarted<7f)
                GUI.Label(new Rect(Screen.width-width-28,28,width,100),Chapter.ToString()=="House"?"03 / LA CASA":Chapter==CampaignChapter.Pressure?"02 / PRESIÓN":Chapter==CampaignChapter.Gallery?"04 / ACOMPAÑAMIENTO":Chapter==CampaignChapter.Archive?"05 / ARCHIVO":"",objectiveStyle);
            if(departureTime>0){var c=GUI.color;GUI.color=new Color(0,0,0,Mathf.Clamp01((departureTime-1)/3));GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=c;}
        }
    }
}
