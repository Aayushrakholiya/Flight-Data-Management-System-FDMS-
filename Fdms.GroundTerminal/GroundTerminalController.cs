/* 
 *  FILE          : GroundTerminalController.cs
 *  PROJECT       :  FDMS (Flight Data Management System)
 *  PROGRAMMER    : Ayushkumar Rakholiya, Jal Shah, Darsh Patel, Viraj Solanki
 *  FIRST VERSION : 2025-11-20
 *  DESCRIPTION   :
 *    This file defines the GroundTerminalController class which manages
 *    the operations of the Ground Terminal application.
 */
using Fdms.GroundTerminal.Models;
using Fdms.Shared;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Fdms.GroundTerminal
{

    //-------------------------------------------------------------
    // CLASS : GroundTerminalController
    // PURPOSE :
    //   Manages the operations of the Ground Terminal application.
    //-------------------------------------------------------------
    public sealed class GroundTerminalController : IDisposable
    {
        private readonly GroundConfig _config;
        private readonly TelemetryCommService _commService;
        private readonly TelemetryDataService _dataService;
        private bool _disposed;

        public event EventHandler<RealTimeTelemetryEventArgs>? RealTimeTelemetryReceived;


        public event EventHandler? InvalidPacketCountChanged;


        public int InvalidPacketCount { get; private set; }


        public int ListenPort => _config.ListenPort;


        /*---------------------------------------------------------
        *  FUNCTION      : DatabaseName
        *  DESCRIPTION   :
        *    Gets the name of the database from the connection string.
        *  PARAMETERS    : None
        *  RETURNS       : The name of the database.
        *---------------------------------------------------------*/
        public string DatabaseName
        {
            get
            {
                try
                {
                    var builder = new SqlConnectionStringBuilder(_config.ConnectionString);
                    return builder.InitialCatalog;
                }
                catch
                {
                    return "(unknown)";
                }
            }
        }

        /*---------------------------------------------------------
        *  FUNCTION      : GroundTerminalController
        *  DESCRIPTION   :
        *    Initializes a new instance of the GroundTerminalController class with the specified configuration.
        *  PARAMETERS    :
        *      config - The GroundConfig instance containing configuration settings.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        public GroundTerminalController(GroundConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _commService = new TelemetryCommService(_config.ListenPort);
            _dataService = new TelemetryDataService(_config.ConnectionString);

            _commService.ValidPacketReceived += CommServiceOnValidPacketReceived;
            _commService.InvalidPacketReceived += CommServiceOnInvalidPacketReceived;
        }

        /*---------------------------------------------------------
        *  FUNCTION      : Start
        *  DESCRIPTION   :
        *    Starts the telemetry communication service.
        *  PARAMETERS    : None
        *  RETURNS       : None
        *---------------------------------------------------------*/
        public void Start()
        {
            _commService.Start();
        }

        /*---------------------------------------------------------
        *  FUNCTION      : Stop
        *  DESCRIPTION   :
        *    Stops the telemetry communication service.
        *  PARAMETERS    : None
        *  RETURNS       : None
        *---------------------------------------------------------*/
        public void Stop()
        {
            _commService.Stop();
        }

        /*---------------------------------------------------------
        *  FUNCTION      : SearchTelemetry
        *  DESCRIPTION   :
        *    Searches telemetry data based on the specified criteria.
        *  PARAMETERS    :
        *      criteria - The criteria to use for searching telemetry data.
        *  RETURNS       : A list of telemetry search results.
        *---------------------------------------------------------*/
        public IList<TelemetrySearchResult> SearchTelemetry(TelemetrySearchCriteria criteria)
        {
            if (criteria == null) throw new ArgumentNullException(nameof(criteria));

            return _dataService.SearchTelemetry(criteria);
        }

        /*---------------------------------------------------------
        *  FUNCTION      : ExportSearchResultsToAscii
        *  DESCRIPTION   :
        *    Exports the search results to an ASCII file.
        *  PARAMETERS    :
        *      criteria - The criteria to use for searching telemetry data.
        *      filePath - The file path to export the ASCII data.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        public void ExportSearchResultsToAscii(TelemetrySearchCriteria criteria, string filePath)
        {
            if (criteria == null) throw new ArgumentNullException(nameof(criteria));

            var results = _dataService.SearchTelemetry(criteria);
            _dataService.ExportResultsToAscii(results, filePath);
        }

        /*---------------------------------------------------------
        *  FUNCTION      : CommServiceOnValidPacketReceived
        *  DESCRIPTION   :
        *    Handles the event when a valid telemetry packet is received.
        *  PARAMETERS    :
        *      sender - The source of the event.
        *      e - The ValidPacketReceivedEventArgs containing event data.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private void CommServiceOnValidPacketReceived(object? sender, ValidPacketReceivedEventArgs e)
        {
            try
            {
                _dataService.SaveValidPacket(e.Packet);

                OnRealTimeTelemetryReceived(new RealTimeTelemetryEventArgs(e.Packet));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[GTS] Error while handling valid packet: {ex.Message}");
                Console.ResetColor();
            }
        }

        /*---------------------------------------------------------
        *  FUNCTION      : CommServiceOnInvalidPacketReceived
        *  DESCRIPTION   :
        *    Handles the event when an invalid telemetry packet is received.
        *  PARAMETERS    :
        *      sender - The source of the event.
        *      e - The InvalidPacketReceivedEventArgs containing event data.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private void CommServiceOnInvalidPacketReceived(object? sender, InvalidPacketReceivedEventArgs e)
        {
            try
            {
                _dataService.SaveInvalidPacket(e);

                InvalidPacketCount++;
                InvalidPacketCountChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[GTS] Error while saving invalid packet: {ex.Message}");
                Console.ResetColor();
            }
        }

        /*---------------------------------------------------------
        *  FUNCTION      : OnRealTimeTelemetryReceived
        *  DESCRIPTION   :
        *    Raises the RealTimeTelemetryReceived event.
        *  PARAMETERS    :
        *      e - The RealTimeTelemetryEventArgs containing event data.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        private void OnRealTimeTelemetryReceived(RealTimeTelemetryEventArgs e)
        {
            RealTimeTelemetryReceived?.Invoke(this, e);
        }
        
        /*---------------------------------------------------------
        *  FUNCTION      : Dispose
        *  DESCRIPTION   :
        *    Disposes of the GroundTerminalController instance and releases resources.
        *  PARAMETERS    : None
        *  RETURNS       : None
        *---------------------------------------------------------*/
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _commService.ValidPacketReceived -= CommServiceOnValidPacketReceived;
            _commService.InvalidPacketReceived -= CommServiceOnInvalidPacketReceived;

            _commService.Dispose();
        }
    }

    //-------------------------------------------------------------
    // CLASS : RealTimeTelemetryEventArgs
    // PURPOSE :
    //   Provides data for the RealTimeTelemetryReceived event.
    //-------------------------------------------------------------
    public sealed class RealTimeTelemetryEventArgs : EventArgs
    {
        public TelemetryPacket Packet { get; }

        /*---------------------------------------------------------
        *  FUNCTION      : RealTimeTelemetryEventArgs
        *  DESCRIPTION   :
        *    Initializes a new instance of the RealTimeTelemetryEventArgs 
        *    class with the specified telemetry packet.
        *  PARAMETERS    :
        *      packet - The TelemetryPacket instance.
        *  RETURNS       : None
        *---------------------------------------------------------*/
        public RealTimeTelemetryEventArgs(TelemetryPacket packet)
        {
            Packet = packet ?? throw new ArgumentNullException(nameof(packet));
        }


        public string TailNumber => Packet.TailNumber;
        public int SequenceNumber => Packet.SequenceNumber;
        public string TimestampRaw => Packet.Telemetry.TimestampRaw;
        public double Altitude => Packet.Telemetry.Altitude;
        public double Pitch => Packet.Telemetry.Pitch;
        public double Bank => Packet.Telemetry.Bank;
        public double Weight => Packet.Telemetry.Weight;
        public double AccelX => Packet.Telemetry.AccelX;
        public double AccelY => Packet.Telemetry.AccelY;
        public double AccelZ => Packet.Telemetry.AccelZ;
        public int Checksum => Packet.Checksum;
    }
}