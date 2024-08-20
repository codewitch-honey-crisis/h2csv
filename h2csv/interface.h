// PlatformIO CLI command line for regenerating:
// ./h2csv include/interface.h /header /output include/init_csv.h

// XBee - ESP32 Interface Protocol
// Rev 00.00.01
// 08/01/23



#ifndef INTERFACE_H
#define INTERFACE_H
#include <stdint.h>
#include <stddef.h>

enum struct STATUS_CODE : int32_t {
    FOTA_FW_ABSENT = -6,
    ACEK9_STATUS_BAD_TOPIC_HANDLER = -5,
    UNKOWNDATAFROMSERVER = -4,
    UNSUPPORTEDTYPEININIT = -3,
    XBEE_NOT_INTIALIZED = -2,
    INVALID_COMMAND = -1,
    SUCCESS = 0,
    XBEE_INITIALIZED = 1,
    XBEE_CELL_CONNECTED = 2,
    FOTA_CHECK_FW = 3,
    FOTA_BEGIN = 4,
    FOTA_DELETE_FILE = 5
};

enum struct COMMAND_ID : int32_t {
    ERROR = -1,
    CONNECT = 1,
    ACKNOWLEDGE,
    DATA,
    STATUS,
    LOG,
    CONNECTION,
    CONFIG,
    COMMAND,
    SUBSCRIBE,
    UPDATE,
    FOTA,
    INIT = 255
};
enum ACE_BOOL : uint32_t {
    ACE_FALSE = 0,
    ACE_TRUE = 1
};

// ====== Command Protocol ======
    // each communication transaction begins with a command id (int)
    // followed by 1 packet as indicated by the command field
    // and then CRC-32 4-byte unsigned int followed by the payload
struct connect_packet
{
    char unitname[16];                          // unitname | 16 bytes | string padded by zeros | Unique Unit Name
    char host[128];                             // host | 128 bytes | string padded by zeros | Server Host URL
    uint32_t port;                              // port | 4 bytes | 32-bit unsigned word | TCP/IP port #
    char username[128];                         // username | 128 bytes | string padded by zeros | MQTT server username
    char password[128];                         // password | 128 bytes | string padded by zeros | MQTT server password
    char lastWillTopic[128];                    // lastWillTopic | 128 bytes | string padded by zeros | MQTT Last Will Topic address
    uint32_t lastWillQos;                       // lastWillQos | 4 bytes | 32-bit unsigned word | Quality of Service Level for the Last Will Message
    char lastWillMessage[128];                  // lastWillMessage | 128 bytes | string padded by zeros | Last Will Message
    ACE_BOOL lastWillRetain;                    // lastWillRetain | ACE_BOOL 32-bit unsigned word | 0 = False, Non-Zero = True
    ACE_BOOL cleanSession;                      // cleanSession | ACE_BOOL 32-bit unsigned word | 0 = False, Non-Zero = True
};

struct subscribe_packet
{
    constexpr static const COMMAND_ID cmd_ID = COMMAND_ID::SUBSCRIBE;
    // uint32_t crc; (prepended to packet)      // CRC | 4 bytes | 32-bit unsigned word  | Indicates the CRC-32 value for the packet
    char topicName[128];                        // topicName | 128 bytes | string padded by zeros | topicName to subscribe to
    char handlerType[128];                      // handlerType | 128 bytes | string padded by zeros | Currently can be "command" or "config"

};


struct acknowledge_packet {
    constexpr static const COMMAND_ID cmd_ID = COMMAND_ID::ACKNOWLEDGE;
    // uint32_t crc; (prepended to packet)      // CRC | 4 bytes | 32-bit unsigned word  | Indicates the CRC-32 value for the packet
    STATUS_CODE status;                         // status | STATUS_CODE 32-bit unsigned word | 0 = Success, Negative Value = Various Errors
};


struct data_packet
{
    constexpr static const COMMAND_ID cmd_ID = COMMAND_ID::DATA;
    // uint32_t crc; (prepended to packet)      // CRC | 4 bytes | 32-bit unsigned word  | Indicates the CRC-32 value for the packet
    char topicName[16];                         //  TopicName | 16 bytes | string padded by zeros | Topic to publish to
    uint32_t qos;                               //  qos | 4 bytes | 32-bit unsigned word | Quality of Service
    ACE_BOOL retainFlag;                        //  retainFlag | ACE_BOOL 32-bit unsigned word | 0 = False, Non-Zero = True
    // ================ Variable Data Below this Point =====================
    char timeStampUTC[32];                      //  timeStampUTC | 32 bytes | string padded by zeros |
    ACE_BOOL powerOn;                           //  powerOn | ACE_BOOL 32-bit unsigned word | 0 = False, Non-Zero = True
    ACE_BOOL ignitionOn;                        //  ignitionOn | ACE_BOOL 32-bit unsigned word | 0 = False, Non-Zero = True
    uint32_t eventCode;                         //  eventCode | 4 bytes | 32-bit unsigned word | 
    uint32_t cellStrength;                      //  cellStrength | 4 bytes | 32-bit unsigned word | 
    ACE_BOOL alarmOn;                           //  alarmOn | ACE_BOOL 32-bit unsigned word | 0 = False, Non-Zero = True
    uint32_t leftTemp;                          //  leftTemp | 4 bytes | 32-bit unsigned word | 
    uint32_t rightTemp;                         //  rightTemp | 4 bytes | 32-bit unsigned word | 
    ACE_BOOL stallSensorPresent;                //  stallSensorPresent | ACE_BOOL 32-bit unsigned word | 0 = False, Non-Zero = True
    uint32_t stallCount;                        //  stallCount | 4 bytes | 32-bit unsigned word | 
    uint32_t batteryVoltage;                    //  batteryVoltage | 4 bytes | 32-bit unsigned word | 
    char doorPopUTC[32];                        //  doorPopUTC | 32 bytes | string padded by zeros |
    uint32_t version;                           //  version | 4 bytes | 32-bit unsigned word | 
    ACE_BOOL newstuff;

};

struct status_packet
{
    constexpr static const COMMAND_ID cmd_ID = COMMAND_ID::STATUS;
    // uint32_t crc; (prepended to packet)      // CRC | 4 bytes | 32-bit unsigned word  | Indicates the CRC-32 value for the packet
    char topicName[16];                         //  TopicName | 16 bytes | string padded by zeros |
    uint32_t qos;                               //  qos | 4 bytes | 32-bit unsigned word | 
    ACE_BOOL retainFlag;                        //  retainFlag | ACE_BOOL 32-bit unsigned word | 0 = False, Non-Zero = True
    // ================ Variable Data Below this Point =====================
    char unitID[32];                            //  unitID | 32 bytes | string padded by zeros |
    char unitname[16];                          //  unitname | 16 bytes | string padded by zeros |
    char unitFirmwareVersion[16];               //  unitFirmwareVersion | 16 bytes | string padded by zeros |
    char ctrlHeadSerialNumber[32];              //  ctrlHeadSerialNumber | 32 bytes | string padded by zeros |
    char modemModel[16];                        //  modemModel | 16 bytes | string padded by zeros |
    char modemFirmwareVersion[32];              //  modemFirmwareVersion | 32 bytes | string padded by zeros |
    char carrierCode[8];                        //  carrierCode | 8 bytes | string padded by zeros |
    char mobileEquipmentID[16];                 //  mobileEquipmentID | 16 bytes | string padded by zeros |
    char integratedCircuitCardID[32];           //  integratedCircuitCardID | 32 bytes | string padded by zeros |
    uint32_t doorPopCount;                      //  doorPopCount | 4 bytes | 32-bit unsigned word | 
};

struct log_packet
{
    constexpr static const COMMAND_ID cmd_ID = COMMAND_ID::LOG;
    // uint32_t crc; (prepended to packet)      // CRC | 4 bytes | 32-bit unsigned word  | Indicates the CRC-32 value for the packet
    char topicName[16];                         //  TopicName | 16 bytes | string padded by zeros |
    uint32_t qos;                               //  qos | 4 bytes | 32-bit unsigned word |
    ACE_BOOL retainFlag;                        //  retainFlag | ACE_BOOL 32-bit unsigned word | 0 = False, Non-Zero = True
    char timeStampUTC[32];                      //  timeStampUTC | 32 bytes | string padded by zeros |
    uint32_t type;                              //  type | 4 bytes | 32-bit unsigned word |
    char message[128];                          //  message | 128 bytes | string padded by zeros |
};

struct connection_packet
{
    constexpr static const COMMAND_ID cmd_ID = COMMAND_ID::CONNECTION;
    // uint32_t crc; (prepended to packet)      // CRC | 4 bytes | 32-bit unsigned word  | Indicates the CRC-32 value for the packet
    char topicName[16];                         // TopicName | 16 bytes | string padded by zeros |
    uint32_t qos;                               // qos | 4 bytes | 32-bit unsigned word | 
    ACE_BOOL retainFlag;                        // retainFlag | ACE_BOOL 32-bit unsigned word | 0 = False, Non-Zero = True
    char status[16];                            // status | 16 bytes | string padded by zeros |
};

struct config_packet
{
    constexpr static const COMMAND_ID cmd_ID = COMMAND_ID::CONFIG;
    // uint32_t crc; (prepended to packet)      // CRC | 4 bytes | 32-bit unsigned word  | Indicates the CRC-32 value for the packet
    char topicName[16];                         // TopicName | 16 bytes | string padded by zeros |
    uint32_t qos;                               // qos  | 4 bytes | 32-bit unsigned word | 
    ACE_BOOL retainFlag;                        // retainFlag | ACE_BOOL 32-bit unsigned word | 0 = False, Non-Zero = True
    // ================ Variable Data Below this Point =====================
    char serverDomain[128];                     // serverDomain | 128 bytes | string padded by zeros |
    char firmwareVersion[16];                   // firmwareVersion | 16 bytes | string padded by zeros |
    char firmwareChecksum[32];                  // firmwareChecksum | 32 bytes | string padded by zeros |
    char firmwareURL[128];                      // firmwareURL | 128 bytes | string padded by zeros |
    uint32_t heartbeatInterval;                 // heartbeatInterval | 4 bytes | 32-bit unsigned word | 
    uint32_t temperatureInterval;               // temperatureInterval | 4 bytes | 32-bit unsigned word | 
    uint32_t temperatureDelta;                  // temperatureDelta | 4 bytes | 32-bit unsigned word | 
    uint32_t connectTimeout;                    // connectTimeout | 4 bytes | 32-bit unsigned word | 
    uint32_t registrationFailureLimit;          // registrationFailureLimit | 4 bytes | 32-bit unsigned word | 
    uint32_t hotAlarmtemperature;               // hotAlarmtemperature | 4 bytes | 32-bit unsigned word | 
    uint32_t coldAlarmTemperature;              // coldAlarmTemperature | 4 bytes | 32-bit unsigned word | 
    uint32_t keepAliveInterval;                 // keepAliveInterval | 4 bytes | 32-bit unsigned word | 
    uint32_t LoggingLevel;                      // LoggingLevel | 4 bytes | 32-bit unsigned word | 
    uint32_t assignmentStatus;                  // assignmentStatus | 4 bytes | 32-bit unsigned word | 
};

struct command_packet
{
    constexpr static const COMMAND_ID cmd_ID = COMMAND_ID::COMMAND;
    // uint32_t crc; (prepended to packet)      // CRC | 4 bytes | 32-bit unsigned word  | Indicates the CRC-32 value for the packet
    char topicName[16];                         // TopicName | 16 bytes | string padded by zeros |
    uint32_t qos;                               // qos | 4 bytes | 32-bit unsigned word | 
    ACE_BOOL retainFlag;                        // retainFlag | ACE_BOOL 32-bit unsigned word | 0 = False, Non-Zero = True
    char command[128];                          // command | 128 bytes | string padded by zeros |
};

struct init_packet
{
    constexpr static const COMMAND_ID cmd_ID = COMMAND_ID::INIT;
    // uint32_t crc; (prepended to packet)      // CRC | 4 bytes | 32-bit unsigned word  | Indicates the CRC-32 value for the packet
    // csv files 
    ACE_BOOL dummy;
};

// A series of these is sent one after another until the entire update firmware file is sent. Any other message cancels the update
// when it's complete a final packet with size of zero should be sent
struct update_packet
{
    constexpr static const COMMAND_ID cmd_ID = COMMAND_ID::UPDATE;
    // uint32_t crc; (prepended to packet)      // CRC | 4 bytes | 32-bit unsigned word  | Indicates the CRC-32 value for the packet
    uint32_t size; // the number of significant bytes in the data field
    uint8_t data[1024]; // preferably 8192
};

// FOTA Packet is used to communicate the FOTA process steps
struct fota_packet
{
    constexpr static const COMMAND_ID cmd_ID = COMMAND_ID::FOTA;
    // uint32_t crc; (prepended to packet)      // CRC | 4 bytes | 32-bit unsigned word  | Indicates the CRC-32 value for the packet
    STATUS_CODE fotaStatus;
};

#endif // INTERFACE_H