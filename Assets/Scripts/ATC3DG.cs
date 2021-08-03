using System;
using System.Runtime.InteropServices;

namespace ATC3DGWrapper
{
    public static class ATC3DG
    {
        public const int DATA_PONITS_NR = 8;
        
        public enum BIRD_ERROR_CODES
        {
            //	ERROR CODE DISPOSITION
            //    |		(Some error codes have been retired.
            //    |      The column below describes which codes 
            //	  |      have been retired and why. O = Obolete,
            //    V      I = handled internally)
            BIRD_ERROR_SUCCESS = 0,                 //00 < > No error	
            BIRD_ERROR_PCB_HARDWARE_FAILURE,        //01 < > indeterminate failure on PCB
            BIRD_ERROR_TRANSMITTER_EEPROM_FAILURE,  //02 <I> transmitter bad eeprom
            BIRD_ERROR_SENSOR_SATURATION_START,     //03 <I> sensor has gone into saturation
            BIRD_ERROR_ATTACHED_DEVICE_FAILURE,     //04 <O> either a sensor or transmitter reports bad
            BIRD_ERROR_CONFIGURATION_DATA_FAILURE,  //05 <O> device EEPROM detected but corrupt
            BIRD_ERROR_ILLEGAL_COMMAND_PARAMETER,   //06 < > illegal PARAMETER_TYPE passed to driver
            BIRD_ERROR_PARAMETER_OUT_OF_RANGE,      //07 < > PARAMETER_TYPE legal, but PARAMETER out of range
            BIRD_ERROR_NO_RESPONSE,                 //08 <O> no response at all from target card firmware
            BIRD_ERROR_COMMAND_TIME_OUT,            //09 < > time out before response from target board
            BIRD_ERROR_INCORRECT_PARAMETER_SIZE,    //10 < > size of parameter passed is incorrect
            BIRD_ERROR_INVALID_VENDOR_ID,           //11 <O> driver started with invalid PCI vendor ID
            BIRD_ERROR_OPENING_DRIVER,              //12 < > couldn't start driver
            BIRD_ERROR_INCORRECT_DRIVER_VERSION,    //13 < > wrong driver version found
            BIRD_ERROR_NO_DEVICES_FOUND,            //14 < > no BIRDs were found anywhere
            BIRD_ERROR_ACCESSING_PCI_CONFIG,        //15 < > couldn't access BIRDs config space
            BIRD_ERROR_INVALID_DEVICE_ID,           //16 < > device ID out of range
            BIRD_ERROR_FAILED_LOCKING_DEVICE,       //17 < > couldn't lock driver
            BIRD_ERROR_BOARD_MISSING_ITEMS,         //18 < > config space items missing
            BIRD_ERROR_NOTHING_ATTACHED,            //19 <O> card found but no sensors or transmitters attached
            BIRD_ERROR_SYSTEM_PROBLEM,              //20 <O> non specific system problem
            BIRD_ERROR_INVALID_SERIAL_NUMBER,       //21 <O> serial number does not exist in system
            BIRD_ERROR_DUPLICATE_SERIAL_NUMBER,     //22 <O> 2 identical serial numbers passed in set command
            BIRD_ERROR_FORMAT_NOT_SELECTED,         //23 <O> data format not selected yet
            BIRD_ERROR_COMMAND_NOT_IMPLEMENTED,     //24 < > valid command, not implemented yet
            BIRD_ERROR_INCORRECT_BOARD_DEFAULT,     //25 < > incorrect response to reading parameter
            BIRD_ERROR_INCORRECT_RESPONSE,          //26 <O> response received, but data,values in error
            BIRD_ERROR_NO_TRANSMITTER_RUNNING,      //27 < > there is no transmitter running
            BIRD_ERROR_INCORRECT_RECORD_SIZE,       //28 < > data record size does not match data format size
            BIRD_ERROR_TRANSMITTER_OVERCURRENT,     //29 <I> transmitter over-current detected
            BIRD_ERROR_TRANSMITTER_OPEN_CIRCUIT,    //30 <I> transmitter open circuit or removed
            BIRD_ERROR_SENSOR_EEPROM_FAILURE,       //31 <I> sensor bad eeprom
            BIRD_ERROR_SENSOR_DISCONNECTED,         //32 <I> previously good sensor has been removed
            BIRD_ERROR_SENSOR_REATTACHED,           //33 <I> previously good sensor has been reattached
            BIRD_ERROR_NEW_SENSOR_ATTACHED,         //34 <O> new sensor attached
            BIRD_ERROR_UNDOCUMENTED,                //35 <I> undocumented error code received from bird
            BIRD_ERROR_TRANSMITTER_REATTACHED,      //36 <I> previously good transmitter has been reattached
            BIRD_ERROR_WATCHDOG,                    //37 < > watchdog timeout
            BIRD_ERROR_CPU_TIMEOUT_START,           //38 <I> CPU ran out of time executing algorithm (start)
            BIRD_ERROR_PCB_RAM_FAILURE,             //39 <I> BIRD on-board RAM failure
            BIRD_ERROR_INTERFACE,                   //40 <I> BIRD PCI interface error
            BIRD_ERROR_PCB_EPROM_FAILURE,           //41 <I> BIRD on-board EPROM failure
            BIRD_ERROR_SYSTEM_STACK_OVERFLOW,       //42 <I> BIRD program stack overrun
            BIRD_ERROR_QUEUE_OVERRUN,               //43 <I> BIRD error message queue overrun
            BIRD_ERROR_PCB_EEPROM_FAILURE,          //44 <I> PCB bad EEPROM
            BIRD_ERROR_SENSOR_SATURATION_END,       //45 <I> Sensor has gone out of saturation
            BIRD_ERROR_NEW_TRANSMITTER_ATTACHED,    //46 <O> new transmitter attached
            BIRD_ERROR_SYSTEM_UNINITIALIZED,        //47 < > InitializeBIRDSystem not called yet
            BIRD_ERROR_12V_SUPPLY_FAILURE,          //48 <I > 12V Power supply not within specification
            BIRD_ERROR_CPU_TIMEOUT_END,             //49 <I> CPU ran out of time executing algorithm (end)
            BIRD_ERROR_INCORRECT_PLD,               //50 < > PCB PLD not compatible with this API DLL
            BIRD_ERROR_NO_TRANSMITTER_ATTACHED,     //51 < > No transmitter attached to this ID
            BIRD_ERROR_NO_SENSOR_ATTACHED,          //52 < > No sensor attached to this ID

            // new error codes added 2/27/03 
            // (Version 1,31,5,01)  multi-sensor, synchronized
            BIRD_ERROR_SENSOR_BAD,                  //53 < > Non-specific hardware problem
            BIRD_ERROR_SENSOR_SATURATED,            //54 < > Sensor saturated error
            BIRD_ERROR_CPU_TIMEOUT,                 //55 < > CPU unable to complete algorithm on current cycle
            BIRD_ERROR_UNABLE_TO_CREATE_FILE,       //56 < > Could not create and open file for saving setup
            BIRD_ERROR_UNABLE_TO_OPEN_FILE,         //57 < > Could not open file for restoring setup
            BIRD_ERROR_MISSING_CONFIGURATION_ITEM,  //58 < > Mandatory item missing from configuration file
            BIRD_ERROR_MISMATCHED_DATA,             //59 < > Data item in file does not match system value
            BIRD_ERROR_CONFIG_INTERNAL,             //60 < > Internal error in config file handler
            BIRD_ERROR_UNRECOGNIZED_MODEL_STRING,   //61 < > Board does not have a valid model string
            BIRD_ERROR_INCORRECT_SENSOR,            //62 < > Invalid sensor type attached to this board
            BIRD_ERROR_INCORRECT_TRANSMITTER,       //63 < > Invalid transmitter type attached to this board

            // new error code added 1/18/05
            // (Version 1.31.5.22) 
            //		multi-sensor, 
            //		synchronized-fluxgate, 
            //		integrating micro-sensor,
            //		flat panel transmitter
            BIRD_ERROR_ALGORITHM_INITIALIZATION,    //64 < > Flat panel algorithm initialization failed

            // new error code for multi-sync
            BIRD_ERROR_LOST_CONNECTION,             //65 < > USB connection has been lost
            BIRD_ERROR_INVALID_CONFIGURATION,       //66 < > Invalid configuration

            // VPD error code
            BIRD_ERROR_TRANSMITTER_RUNNING,         //67 < > TX running while reading/writing VPD

            BIRD_ERROR_MAXIMUM_VALUE = 0x7F         //	     ## value = number of error codes ##
        };

        private static bool isInited = false;
        private static bool[] isSensorAttached;        
        private static int attachedSensorsNr = 0;

        public static BIRD_ERROR_CODES init()
        {
            if (isInited)
            {
                // close the bird system
            }

            BIRD_ERROR_CODES err;
            if ((err = (BIRD_ERROR_CODES)DllImporter.InitializeBIRDSystem()) != BIRD_ERROR_CODES.BIRD_ERROR_SUCCESS)
                return err;

            ushort transmitterId = 0;
            if ((err = (BIRD_ERROR_CODES)DllImporter.SetSystemParameter((int)DllImporter.SYSTEM_PARAMETER_TYPE.SELECT_TRANSMITTER, ref transmitterId, sizeof(ushort))) != BIRD_ERROR_CODES.BIRD_ERROR_SUCCESS)
                return err;            

            uint isMetric = 1;
            if ((err = (BIRD_ERROR_CODES)DllImporter.SetSystemParameter((int)DllImporter.SYSTEM_PARAMETER_TYPE.METRIC, ref isMetric, sizeof(uint))) != BIRD_ERROR_CODES.BIRD_ERROR_SUCCESS)
                return err;

            DllImporter.SYSTEM_CONFIGURATION systemConfigStruct;
            if ((err = (BIRD_ERROR_CODES)DllImporter.GetBIRDSystemConfiguration(out systemConfigStruct)) != BIRD_ERROR_CODES.BIRD_ERROR_SUCCESS)
                return err;
            isSensorAttached = new bool[systemConfigStruct.numberSensors];
            // Debug.Log("[3D Guidance Params] Measurement Rate: " + systemConfigStruct.measurementRate);
            // Debug.Log("[3D Guidance Params] Transmitters Number: " + systemConfigStruct.numberTransmitters);
            // Debug.Log("[3D Guidance Params] Running Transmitter ID: " + systemConfigStruct.transmitterIDRunning);                        
            //Debug.Log("[3D Guidance Params] Is Metric: " + systemConfigStruct.metric);            

            int dataFormat = (int)DllImporter.DATA_FORMAT_TYPE.DOUBLE_POSITION_ANGLES_TIME_Q;
            for (ushort sensorIdx = 0; sensorIdx < systemConfigStruct.numberSensors; sensorIdx++)
            {
                // TODO: Understand why sensorConfigStruct.attached was set to false while sensorStatus was set to 0
                //DllImporter.SENSOR_CONFIGURATION sensorConfigStruct;
                //if ((err = (BIRD_ERROR_CODES)DllImporter.GetSensorConfiguration(sensorIdx, out sensorConfigStruct)) != BIRD_ERROR_CODES.BIRD_ERROR_SUCCESS)
                //   return err;

                ulong sensorStatus = DllImporter.GetSensorStatus(sensorIdx);
                string s = Convert.ToString((long)sensorStatus, 2);
                if ((sensorStatus & 1) == 0)
                {
                    if ((err = (BIRD_ERROR_CODES)DllImporter.SetSensorParameter(sensorIdx, (int)DllImporter.SENSOR_PARAMETER_TYPE.DATA_FORMAT, ref dataFormat, sizeof(int))) != BIRD_ERROR_CODES.BIRD_ERROR_SUCCESS)
                        return err;                    
                    
                    isSensorAttached[sensorIdx] = true;
                    attachedSensorsNr++;
                }
                else
                    isSensorAttached[sensorIdx] = false;
            }        
            if (attachedSensorsNr == 0)
                return BIRD_ERROR_CODES.BIRD_ERROR_NO_SENSOR_ATTACHED;

            isInited = true;
            return BIRD_ERROR_CODES.BIRD_ERROR_SUCCESS;
        }

        public static BIRD_ERROR_CODES getAsynchronousDataSingleSensor(ushort sensorIdx, out float[] dataOut)
        {
            dataOut = new float[DATA_PONITS_NR];

            if (!isInited)
                return BIRD_ERROR_CODES.BIRD_ERROR_SYSTEM_UNINITIALIZED;
            
            if (!isSensorAttached[sensorIdx])
                return BIRD_ERROR_CODES.BIRD_ERROR_NO_SENSOR_ATTACHED;

            DllImporter.ATC3DG_DATA_RECROD pRecord;
            BIRD_ERROR_CODES err;
            if ((err = (BIRD_ERROR_CODES)DllImporter.GetAsynchronousRecord(sensorIdx, out pRecord, Marshal.SizeOf(typeof(DllImporter.ATC3DG_DATA_RECROD)))) != BIRD_ERROR_CODES.BIRD_ERROR_SUCCESS)
                return err;
                       
            dataOut[0] = (float)pRecord.x / 10.0f;
            dataOut[1] = (float)pRecord.y / 10.0f;
            dataOut[2] = (float)pRecord.z / 10.0f;
            dataOut[3] = (float)pRecord.a;
            dataOut[4] = (float)pRecord.e;
            dataOut[5] = (float)pRecord.r;            
            dataOut[6] = (float)pRecord.time;
            dataOut[7] = pRecord.quality;
            
            return BIRD_ERROR_CODES.BIRD_ERROR_SUCCESS;
        }

        public static BIRD_ERROR_CODES getAsynchronousData(out float[] dataOut)
        {
            dataOut = new float[DATA_PONITS_NR * attachedSensorsNr];

            if (!isInited)
                return BIRD_ERROR_CODES.BIRD_ERROR_SYSTEM_UNINITIALIZED;

            DllImporter.ATC3DG_DATA_RECROD[] pRecords = new DllImporter.ATC3DG_DATA_RECROD[attachedSensorsNr];
            int attachedSensorsIt = 0;
            BIRD_ERROR_CODES err;            
            for (ushort sensorIdx = 0; sensorIdx < isSensorAttached.Length; sensorIdx++)
            {                                
                if (isSensorAttached[sensorIdx])
                {                    
                    if ((err = (BIRD_ERROR_CODES)DllImporter.GetAsynchronousRecord(sensorIdx, out pRecords[attachedSensorsIt++], Marshal.SizeOf(typeof(DllImporter.ATC3DG_DATA_RECROD)))) != BIRD_ERROR_CODES.BIRD_ERROR_SUCCESS)
                        return err;
                }
            }

            for (int attachedSensorIdx = 0; attachedSensorIdx < attachedSensorsNr; attachedSensorIdx++)
            {
                dataOut[attachedSensorIdx*DATA_PONITS_NR    ] = (float)pRecords[attachedSensorIdx].x / 10.0f;
                dataOut[attachedSensorIdx*DATA_PONITS_NR + 1] = (float)pRecords[attachedSensorIdx].y / 10.0f;
                dataOut[attachedSensorIdx*DATA_PONITS_NR + 2] = (float)pRecords[attachedSensorIdx].z / 10.0f;
                dataOut[attachedSensorIdx*DATA_PONITS_NR + 3] = (float)pRecords[attachedSensorIdx].a;
                dataOut[attachedSensorIdx*DATA_PONITS_NR + 4] = (float)pRecords[attachedSensorIdx].e;
                dataOut[attachedSensorIdx*DATA_PONITS_NR + 5] = (float)pRecords[attachedSensorIdx].r;                
                dataOut[attachedSensorIdx*DATA_PONITS_NR + 6] = (float)pRecords[attachedSensorIdx].time;
                dataOut[attachedSensorIdx*DATA_PONITS_NR + 7] = pRecords[attachedSensorIdx].quality;
            }

            return BIRD_ERROR_CODES.BIRD_ERROR_SUCCESS;
        }

        public static BIRD_ERROR_CODES closeBIRDSystem()
        {            
            return (BIRD_ERROR_CODES)DllImporter.CloseBIRDSystem();           
        }

        private static class DllImporter
        {                        
            public enum AGC_MODE_TYPE
            {
                TRANSMITTER_AND_SENSOR_AGC,     // Old style normal addressing mode
                SENSOR_AGC_ONLY                 // Old style extended addressing mode
            };

            public enum SYSTEM_PARAMETER_TYPE
            {
                SELECT_TRANSMITTER,     // short int equal to transmitterID of selected transmitter
                POWER_LINE_FREQUENCY,   // double value (range is hardware dependent)
                AGC_MODE,               // enumerated constant of type AGC_MODE_TYPE
                MEASUREMENT_RATE,       // double value (range is hardware dependent)
                MAXIMUM_RANGE,          // double value (range is hardware dependent)
                METRIC,                 // boolean value to select metric units for position
                VITAL_PRODUCT_DATA,     // single byte parameter to be read/write from VPD section of board EEPROM
                POST_ERROR,             // system (board 0) POST_ERROR_PARAMETER
                DIAGNOSTIC_TEST,        // system (board 0) DIAGNOSTIC_TEST_PARAMETER
                REPORT_RATE,            // single byte 1-127			
                COMMUNICATIONS_MEDIA,   // Media structure
                LOGGING,                // Boolean
                RESET,                  // Boolean
                AUTOCONFIG,             // BYTE 1-127
                AUXILIARY_PORT,         // structure of type AUXILIARY_PORT_PARAMETERS
                COMMUTATION_MODE,       // boolean value to select commutation of sensor data for interconnect pickup rejection
                END_OF_LIST             // end of list place holder
            };

            public enum SENSOR_PARAMETER_TYPE
            {
                DATA_FORMAT,            // enumerated constant of type DATA_FORMAT_TYPE
                ANGLE_ALIGN,            // structure of type DOUBLE_ANGLES_RECORD
                HEMISPHERE,             // enumerated constant of type HEMISPHERE_TYPE
                FILTER_AC_WIDE_NOTCH,   // boolean value to select/deselect filter
                FILTER_AC_NARROW_NOTCH, // boolean value to select/deselect filter
                FILTER_DC_ADAPTIVE,     // double value in range 0.0 (no filtering) to 1.0 (max)
                FILTER_ALPHA_PARAMETERS,// structure of type ADAPTIVE_PARAMETERS
                FILTER_LARGE_CHANGE,    // boolean value to select/deselect filter
                QUALITY,                // structure of type QUALITY_PARAMETERS
                SERIAL_NUMBER_RX,       // attached sensor's serial number
                SENSOR_OFFSET,          // structure of type DOUBLE_POSITION_RECORD
                VITAL_PRODUCT_DATA_RX,  // single byte parameter to be read/write from VPD section of sensor EEPROM
                VITAL_PRODUCT_DATA_PREAMP,  // single byte parameter to be read/write from VPD section of preamp EEPROM
                MODEL_STRING_RX,        // 11 byte null terminated character string
                PART_NUMBER_RX,         // 16 byte null terminated character string
                MODEL_STRING_PREAMP,    // 11 byte null terminated character string
                PART_NUMBER_PREAMP,     // 16 byte null terminated character string
                PORT_CONFIGURATION,     // enumerated constant of type PORT_CONFIGURATION_TYPE
                END_OF_RX_LIST
            };

            public enum DATA_FORMAT_TYPE
            {
                NO_FORMAT_SELECTED = 0,

                // SHORT (integer) formats
                SHORT_POSITION,
                SHORT_ANGLES,
                SHORT_MATRIX,
                SHORT_QUATERNIONS,
                SHORT_POSITION_ANGLES,
                SHORT_POSITION_MATRIX,
                SHORT_POSITION_QUATERNION,

                // DOUBLE (floating point) formats
                DOUBLE_POSITION,
                DOUBLE_ANGLES,
                DOUBLE_MATRIX,
                DOUBLE_QUATERNIONS,
                DOUBLE_POSITION_ANGLES,     // system default
                DOUBLE_POSITION_MATRIX,
                DOUBLE_POSITION_QUATERNION,

                // DOUBLE (floating point) formats with time stamp appended
                DOUBLE_POSITION_TIME_STAMP,
                DOUBLE_ANGLES_TIME_STAMP,
                DOUBLE_MATRIX_TIME_STAMP,
                DOUBLE_QUATERNIONS_TIME_STAMP,
                DOUBLE_POSITION_ANGLES_TIME_STAMP,
                DOUBLE_POSITION_MATRIX_TIME_STAMP,
                DOUBLE_POSITION_QUATERNION_TIME_STAMP,

                // DOUBLE (floating point) formats with time stamp appended and quality #
                DOUBLE_POSITION_TIME_Q,
                DOUBLE_ANGLES_TIME_Q,
                DOUBLE_MATRIX_TIME_Q,
                DOUBLE_QUATERNIONS_TIME_Q,
                DOUBLE_POSITION_ANGLES_TIME_Q,
                DOUBLE_POSITION_MATRIX_TIME_Q,
                DOUBLE_POSITION_QUATERNION_TIME_Q,

                // These DATA_FORMAT_TYPE codes contain every format in a single structure
                SHORT_ALL,
                DOUBLE_ALL,
                DOUBLE_ALL_TIME_STAMP,
                DOUBLE_ALL_TIME_STAMP_Q,
                DOUBLE_ALL_TIME_STAMP_Q_RAW,    // this format contains a raw data matrix and
                                                // is for factory use only...

                // DOUBLE (floating point) formats with time stamp appended, quality # and button
                DOUBLE_POSITION_ANGLES_TIME_Q_BUTTON,
                DOUBLE_POSITION_MATRIX_TIME_Q_BUTTON,
                DOUBLE_POSITION_QUATERNION_TIME_Q_BUTTON,

                // New types for button and wrapper
                DOUBLE_POSITION_ANGLES_MATRIX_QUATERNION_TIME_Q_BUTTON,

                MAXIMUM_FORMAT_CODE
            };

            public struct SYSTEM_CONFIGURATION
            {
                public double measurementRate;
                public double powerLineFrequency;
                public double maximumRange;
                public AGC_MODE_TYPE agcMode;
                public int numberBoards;
                public int numberSensors;
                public int numberTransmitters;
                public int transmitterIDRunning;
                public bool metric;
            }

            public enum DEVICE_TYPES
            {
                STANDARD_SENSOR,                // 25mm standard sensor
                TYPE_800_SENSOR,                // 8mm sensor
                STANDARD_TRANSMITTER,           // TX for 25mm sensor
                MINIBIRD_TRANSMITTER,           // TX for 8mm sensor
                SMALL_TRANSMITTER,              // "compact" transmitter
                TYPE_500_SENSOR,                // 5mm sensor
                TYPE_180_SENSOR,                // 1.8mm microsensor
                TYPE_130_SENSOR,                // 1.3mm microsensor
                TYPE_TEM_SENSOR,                // 1.8mm, 1.3mm, 0.Xmm microsensors
                UNKNOWN_SENSOR,                 // default
                UNKNOWN_TRANSMITTER,            // default
                TYPE_800_BB_SENSOR,             // BayBird sensor
                TYPE_800_BB_STD_TRANSMITTER,    // BayBird standard TX
                TYPE_800_BB_SMALL_TRANSMITTER,  // BayBird small TX
                TYPE_090_BB_SENSOR              // Baybird 0.9 mm sensor
            };

            public struct SENSOR_CONFIGURATION
            {
                public ulong serialNumber;
                public ushort boardNumber;
                public ushort channelNumber;
                public DEVICE_TYPES type;
                public bool attached;
            }           

            public struct ATC3DG_DATA_RECROD
            {
                public double x;
                public double y;
                public double z;
                public double a;
                public double e;
                public double r;
                public double time;
                public ushort quality;                
            }

            [DllImport("ATC3DG64")]
            public static extern int InitializeBIRDSystem();

            [DllImport("ATC3DG64")]
            public static extern int GetBIRDSystemConfiguration(out SYSTEM_CONFIGURATION configStruct);

            [DllImport("ATC3DG64")]
            public static extern int GetSensorConfiguration(ushort sensorID, out SENSOR_CONFIGURATION sensorConfiguration);

            [DllImport("ATC3DG64")]
            public static extern ulong GetSensorStatus(ushort sensorID);

            [DllImport("ATC3DG64")]
            public static extern int CloseBIRDSystem();

            [DllImport("ATC3DG64")]
            public static extern int GetAsynchronousRecord(ushort sensorID, out ATC3DG_DATA_RECROD pRecord, int recordSize);

            [DllImport("ATC3DG64")]
            public static extern int SetSystemParameter(int parameterType, ref ushort pBuffer, int bufferSize);

            [DllImport("ATC3DG64")]
            public static extern int SetSystemParameter(int parameterType, ref uint pBuffer, int bufferSize);

            [DllImport("ATC3DG64")]
            public static extern int SetSensorParameter(ushort sensorID, SENSOR_PARAMETER_TYPE parameterType, ref int pBuffer, int bufferSize);

            [DllImport("ATC3DG64")]
            public static extern int GetSensorParameter(ushort sensorID, SENSOR_PARAMETER_TYPE parameterType, out int pRecord, int recordSize);
        }
    }
}
