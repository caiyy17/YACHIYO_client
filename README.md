# YACHIYO Client

A Unity client for real-time AI assistant interaction. Supports voice input, text display, audio playback, and character animation (both traditional and SMPL-H motion), with WebSocket and WebRTC server connections. Designed as the frontend counterpart to [YACHIYO Server](https://github.com/caiyy17/YACHIYO_server).

## Quick Start

1. Open project in **Unity 6000.3.10f1** (URP)
2. Configure server address in GameStart scene settings
3. Build or play from one of the sample scenes

## Scenes

| Scene | Description |
|-------|-------------|
| `GameStart` | Launcher with settings input and scene loading |
| `SampleScene3D_Default` | Standard action system with UnityChan character |
| `SampleScene3D_Live` | Live mode — external interaction via gateway, SMPL-H motion + UnityChan |
| `SampleScene3D_Smpl` | Same as Default but with SMPL-H motion instead of traditional animation |
| `SampleScene3D_WebRTC` | Real-time bidirectional audio/video via WebRTC |

## Architecture

```
Microphone → Pipeline Modules → Server (WebSocket / WebRTC) → Pipeline Modules → Character
```

The client uses a **modular processing pipeline** (`ProcessingPipeline`) where messages flow through chained modules via thread-safe queues. Each module can capture specific signals, process data, or forward messages downstream.

### Pipeline Modules

| Module | Function |
|--------|----------|
| KWSModule | Keyword spotting (optional voice activation) |
| VADModule | Voice activity detection, emits recording_start/stop |
| RecordingModule | Captures microphone audio, encodes to base64 WAV |
| DirectSendModule | Sends predefined text messages (bypasses recording) |
| WebSocketClientModule | WebSocket connection to server |
| WebRTCClientModule | WebRTC connection with audio/video tracks + DataChannels |
| AudioModule | Decodes base64 audio responses, sequential playback |
| ContentModule | Text display via TextMesh Pro |
| ActionModule | Single-field consumer — extracts one configured JSON field and fires a UnityEvent |

### Message Routing

- `destination = -2`: deliver to next module (default)
- `destination = -1`: skip all modules, deliver to pipeline tail
- `destination = N`: deliver to module at index N
- Unrecognized signals are auto-forwarded downstream

## Subsystems

### Audio Recording (`Recorder/`)

- **MicrophoneManager**: Singleton managing continuous mic input (16kHz)
- **VoiceDetector**: Loudness-based VAD with configurable thresholds
- **KeywordDetector**: Optional keyword spotting activation

### Streaming (`Stream/`)

- **WebRTCClient**: Bidirectional audio/video via WebRTC
  - Video source: `None` (blank placeholder) / `Camera` (Unity Camera) / `Webcam` (physical webcam)
  - Configurable resolution and FPS (must match server pipeline config)
  - Inspector shows real-time send/receive stats via WebRTC GetStats API
  - Audio fixed at 48kHz (Unity WebRTC constraint)
- **WebSocketClient**: Alternative WebSocket transport

### NDI Output (`Scripts/`)

- **NdiSendManager**: Creates NDI send streams from Unity Cameras via KlakNDI. Each camera becomes a separate NDI source.

### Model Control (`ModelControl/`)

- **ActionMap** (ScriptableObject): Reusable mapping from action keys → variants with layer-based priority
- **Anim3D**: Multi-target animation controller (motion, expression, blink, lip-sync)

### SMPL-H Motion System (`SmplhMotion/`)

- **SmplhMotionData**: Decoded SMPL-H parameters (poses/trans/betas as float arrays)
- **SmplhConverter**: Axis-angle → quaternion conversion with coordinate X-mirroring
- **SmplhMotionPlayer**: Frame-buffer playback with crossfade blending and auto-refill idle
- **IdleInitializer**: Loads pre-baked idle motion from `Resources/idle_motion.json`

### State Machine (`States/`)

| State | Description |
|-------|-------------|
| IdleState | Waiting; listens for activation signals |
| ReadyState | Preparation (checks AudioModule & character ready) |
| ListeningState | Recording in progress (VAD active) |
| AnsweringState | Server processing + response playback |

Managed by **YYStateManager** via **SignalManager** event routing.

### Settings (`Setting/`)

- **GameSettingsData**: Server URLs, user ID, pipeline config, character selection
- **CharacterSettingsData**: Per-character model, animator, action dict references
- **SetupPipeline**: Server health check → register → init_pipeline workflow

## Client-Server Flow

### WebSocket Mode

```
1. POST /register/                 → Register client
2. POST /init_pipeline/{client_id} → Load pipeline config
3. WS   /ws/{client_id}           → Bidirectional messaging
```

### WebRTC Mode

```
1. POST /register/                 → Register client
2. POST /init_pipeline/{client_id} → Load pipeline config
3. POST /offer/{client_id}        → SDP + video params (fps/width/height)
4. Bidirectional: audio tracks + video tracks + DataChannels
```

Video parameters (`video_fps`, `video_width`, `video_height`) must be consistent between the Unity client, server defaults, and pipeline config (`frame_splitter` node).

## Key Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Universal RP | 17.3.0 | Rendering pipeline |
| WebRTC | 3.0.0-pre.7 | Real-time communication |
| KlakNDI | 2.1.5 | NDI video output |
| Input System | 1.18.0 | Input handling |
| Newtonsoft JSON | 3.2.2 | JSON serialization |

## Project Structure

```
Assets/Custom/
├── YACHIYO/
│   ├── ModelControl/    — Anim3D (multi-target animation), ActionMap (ScriptableObject)
│   ├── Pipeline/        — ProcessingPipeline, ProcessingModule, all modules
│   ├── Recorder/        — MicrophoneManager, VoiceDetector, KeywordDetector
│   ├── Setting/         — Game/character settings, pipeline setup
│   ├── SmplhMotion/     — SMPL-H converter, player, idle initializer
│   ├── States/          — State machine (Idle/Ready/Listening/Answering)
│   ├── Stream/          — WebRTC/WebSocket clients
│   └── Utils/           — SignalManager, custom event types, utilities
├── Scripts/
│   ├── GameStart/       — Launcher UI, scene loading
│   ├── SampleScene/     — Scene-specific UI scripts
│   └── NdiSendManager   — NDI camera output
Assets/Models/           — ActionMap assets, settings data
Assets/UXUI/             — UI components, icons, backgrounds
```

## License

This project's source code is licensed under the [MIT License](LICENSE).

### Third-Party Assets

- **Unity-Chan** (`Assets/UnityChan/`): © Unity Technologies Japan/UCL — [Unity-Chan License Terms (UCL 2.02)](https://unity-chan.com/contents/license_jp/)
- **40+ Simple Icons - Free** (`Assets/40+ Simple Icons - Free/`): [Unity Asset Store EULA](https://unity.com/legal/as-terms)
- **SmileySans / Source Han Sans** (`Assets/Fonts/`): SIL Open Font License (OFL)
