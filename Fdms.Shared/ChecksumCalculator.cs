/* 
 *  FILE          : ChecksumCalculator.cs
 *  PROJECT       :  FDMS (Flight Data Management System)
 *  PROGRAMMER    : Ayushkumar Rakholiya, Jal Shah, Darsh Patel, Viraj Solanki
 *  FIRST VERSION : 2025-11-20
 *  DESCRIPTION   :
    *    This file defines the ChecksumCalculator class and encapsulates all 
    *    variables, constants, and logic for handling checksum calculations for telemetry records.
 */

using System;

namespace Fdms.Shared
{
    //-------------------------------------------------------------
    // CLASS : ChecksumCalculator
    // PURPOSE :
    //   Provides functionality to calculate checksums for telemetry records.
    //-------------------------------------------------------------
    public static class ChecksumCalculator
    {
        /*---------------------------------------------------------
         *  FUNCTION      : CalculateChecksum
         *  DESCRIPTION   :
         *    Calculates the checksum for a given TelemetryRecord instance.
         *  PARAMETERS    : 
         *      record - The TelemetryRecord instance to calculate the checksum for.
         *  RETURNS       : The calculated checksum as an integer.
         *---------------------------------------------------------*/
        public static int CalculateChecksum(TelemetryRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            return CalculateChecksum(record.Altitude, record.Pitch, record.Bank);
        }

        /*---------------------------------------------------------
         *  FUNCTION      : CalculateChecksum
         *  DESCRIPTION   :
         *    Calculates the checksum based on altitude, pitch, and bank values.
         *  PARAMETERS    : 
         *      altitude - The altitude value.
         *      pitch - The pitch value.
         *      bank - The bank value.
         *  RETURNS       : The calculated checksum as an integer.
         *---------------------------------------------------------*/
        public static int CalculateChecksum(double altitude, double pitch, double bank)
        {
            double average = (altitude + pitch + bank) / 3.0;

            int checksum = (int)Math.Round(average, MidpointRounding.AwayFromZero);

            return checksum;
        }
    }
}
