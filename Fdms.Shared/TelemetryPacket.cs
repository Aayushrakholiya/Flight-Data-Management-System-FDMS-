/* 
 *  FILE          : TelemetryPacket.cs
 *  PROJECT       :  FDMS (Flight Data Management System)
 *  PROGRAMMER    : Ayushkumar Rakholiya, Jal Shah, Darsh Patel, Viraj Solanki
 *  FIRST VERSION : 2025-11-20
 *  DESCRIPTION   :
    *    This file defines the TelemetryPacket class and encapsulates all 
    *    variables, constants, and logic for handling telemetry data records.
 */
using System;

namespace Fdms.Shared
{
    //-------------------------------------------------------------
    // CLASS : TelemetryPacket
    // PURPOSE :
    //   Encapsulates a telemetry data packet with associated metadata.
    //-------------------------------------------------------------
    public sealed class TelemetryPacket
    {

        public string TailNumber { get; }

        public int SequenceNumber { get; }

        public TelemetryRecord Telemetry { get; }

        public int Checksum { get; }

        /*---------------------------------------------------------
        *  FUNCTION      : TelemetryPacket
        *  DESCRIPTION   :
        *    initialize a TelemetryPacket instance with
        *    provided metadata and telemetry record.
        *  PARAMETERS    : None
        *---------------------------------------------------------*/
        public TelemetryPacket(string tailNumber, int sequenceNumber, TelemetryRecord telemetry, int checksum)
        {
            if (string.IsNullOrWhiteSpace(tailNumber))
            {
                throw new ArgumentException("Tail number cannot be null or empty.", nameof(tailNumber));
            }

            if (sequenceNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sequenceNumber),
                    "Sequence number must be non-negative.");
            }

            Telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));

            TailNumber = tailNumber.Trim();
            SequenceNumber = sequenceNumber;
            Checksum = checksum;
        }

        /*---------------------------------------------------------
        *  FUNCTION      : ToString
        *  DESCRIPTION   :
        *    Provides a string representation of the TelemetryPacket instance.
        *  PARAMETERS    : None
        *  RETURNS       : A string representation of the TelemetryPacket.
        *---------------------------------------------------------*/
        public override string ToString()
        {
            return $"{TailNumber} #{SequenceNumber} | Alt={Telemetry.Altitude}, Pitch={Telemetry.Pitch}, Bank={Telemetry.Bank}, Checksum={Checksum}";
        }
    }
}
