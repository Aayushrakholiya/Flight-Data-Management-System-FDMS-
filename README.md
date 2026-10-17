<div align="center">

# ✈️ FDMS — Flight Data Management System

**A real-time aircraft telemetry pipeline: an Air Traffic Simulator streams live flight data over UDP to a Ground Terminal that validates, stores, and displays it.**

C# · .NET 8 · UDP Sockets · SQL Server · WinForms

<br>

![.NET 8](https://img.shields.io/badge/.NET_8-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white)
![UDP Sockets](https://img.shields.io/badge/UDP_Sockets-blue?style=flat-square)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white)
![WinForms](https://img.shields.io/badge/WinForms-0078D4?style=flat-square&logo=.net&logoColor=white)

</div>

---

## 📌 Table of Contents

- [About](#-about)
- [How It Works](#-how-it-works)
- [Project Structure](#%EF%B8%8F-project-structure)
- [The Telemetry Packet](#-the-telemetry-packet)
- [Getting Started](#-getting-started)
- [Configuration](#%EF%B8%8F-configuration)
- [Team](#-team)

## 👋 About

FDMS simulates the ground side of flight data monitoring. An **Air Traffic Simulator (ATS)** replays recorded sensor telemetry from three aircraft (C-FGAX, C-GEFC, C-QWWT) and streams it packet-by-packet across the network. A **Ground Terminal** receives every packet, verifies its integrity, writes the data to a SQL Server database, and shows live readings in a desktop dashboard — all in real time.

It's the kind of system airlines actually use to watch aircraft health from the ground, built as a team software-engineering project.

## 🔄 How It Works

```text
┌──────────────────────┐                        ┌─────────────────────────────┐
│  Fdms.Ats            │      UDP :5000         │  Fdms.GroundTerminal        │
│  (Air Traffic        │  ──────────────────▶   │  (Ground Terminal)          │
│   Simulator)         │   serialized packets   │                             │
│                      │                        │  1. Receive packet          │
│  • Reads telemetry   │                        │  2. Validate checksum       │
│    files (3 aircraft)│                        │  3. Persist → SQL Server    │
│  • Serializes packets│                        │  4. Update live dashboard   │
│  • Sends at interval │                        │     (WinForms UI)           │
└──────────────────────┘                        └─────────────────────────────┘
                     both sides share Fdms.Shared
                     (packet format · serializer · checksum)
```

- **Fdms.Ats** — the sender. Reads timestamped telemetry records from per-aircraft text files, wraps each record in a packet, and fires it over UDP to the ground terminal on a configurable interval.
- **Fdms.GroundTerminal** — the receiver. A UDP listener validates each packet's checksum, stores the telemetry in SQL Server, and pushes live readings to a WinForms dashboard.
- **Fdms.Shared** — the contract. Packet model, binary serializer, and checksum calculator shared by both sides so the wire format always matches.

## 🗂️ Project Structure

```text
FDMS_Group15/
├── Fdms.Ats/                 🛫 Air Traffic Simulator (console sender)
│   ├── Program.cs            Entry point
│   ├── AtsSender.cs          Packet send loop
│   ├── TelemetryFileReader.cs Parses raw telemetry files
│   ├── AtsConfig.cs          Host, port, tail number, interval
│   └── C-*.txt               Recorded telemetry (C-FGAX, C-GEFC, C-QWWT)
├── Fdms.GroundTerminal/      🖥️ Ground Terminal (Windows desktop app)
│   ├── Program.cs            Entry point
│   ├── MainForm.cs           Live telemetry dashboard (WinForms)
│   ├── TelemetryCommService.cs UDP receive + validation + events
│   ├── TelemetryDataService.cs Database persistence layer
│   ├── GroundTerminalController.cs App orchestration
│   └── GroundConfig.cs       Listen port + SQL connection string
├── Fdms.Shared/              📦 Shared wire contract
│   ├── TelemetryPacket.cs    Packet: tail number + sequence + record + checksum
│   ├── TelemetryRecord.cs    One sensor reading (timestamp, accel, altitudes…)
│   ├── PacketSerializer.cs   Binary encode/decode
│   └── ChecksumCalculator.cs Packet integrity
└── FDMS_Group15.sln          Visual Studio solution
```

## 📦 The Telemetry Packet

Every reading travels as a self-contained packet:

| Field | Purpose |
| --- | --- |
| `TailNumber` | Which aircraft sent it (e.g. C-FGAX) |
| `SequenceNumber` | Ordering + gap detection |
| `TelemetryRecord` | Timestamp, 3-axis acceleration, altitude & pressure readings |
| `Checksum` | Integrity check — corrupt packets are rejected on arrival |

## 🚀 Getting Started

**Prerequisites:** .NET 8 SDK (the ground terminal is a Windows desktop app, so run it on Windows) and a SQL Server instance.

```bash
git clone https://github.com/Aayushrakholiya/Flight-Data-Management-System-FDMS-.git
cd Flight-Data-Management-System-FDMS-

# build everything
dotnet build FDMS_Group15.sln

# terminal 1 — start the ground terminal first
dotnet run --project Fdms.GroundTerminal

# terminal 2 — start streaming telemetry
dotnet run --project Fdms.Ats
```

Watch the dashboard light up as packets arrive.

## ⚙️ Configuration

| Setting | Where | Default |
| --- | --- | --- |
| Ground terminal listen port | `Fdms.Ats/AtsConfig.cs` → `GroundPort` | `5000` |
| ATS send interval | `Fdms.Ats/AtsConfig.cs` | configurable, seconds |
| Aircraft tail number | `Fdms.Ats/AtsConfig.cs` | per run |
| SQL Server connection string | `Fdms.GroundTerminal/GroundConfig.cs` | required |

## 👥 Team

Built by **Group 15** — Ayushkumar Rakholiya · Jal Shah · Darsh Patel · Viraj Solanki

---

<div align="center">

⭐ **Star this repo if you find it useful** ⭐

</div>
