using System;
using System.Collections.Generic;

namespace SniffLogFW
{
    public class ModbusDecoder
    {
        public static List<ModbusPacket> Decode(byte[] buffer)
        {
            List<ModbusPacket> result = new List<ModbusPacket>();

            if (buffer.Length < 4)
            {
                return null;
            }

            ushort function = buffer[1];

            switch (function)
            {
                case 0x01:
                case 0x03:
                case 0x05:
                case 0x06:
                    if (buffer.Length == 8)
                        result.Add(SingleDecode(buffer));
                    else if (buffer[2] == buffer.Length - 5)
                        result.Add(SingleDecode(buffer));
                    else
                    {
                        if (buffer.Length > 8)
                        {
                            byte[] request = new byte[8];
                            byte[] response = new byte[buffer.Length - 8];
                            Array.Copy(buffer, request, 8);
                            Array.Copy(buffer, 8, response, 0, buffer.Length - 8);
                            result.Add(SingleDecode(request));
                            List<ModbusPacket> rest = Decode(response);
                            if (rest != null)
                                result.AddRange(rest);
                        }
                        else
                            result.Add(SingleDecode(buffer));
                    }
                    break;
                case 0x0F:
                case 0x10:
                    if (buffer[6] == buffer.Length - 5)
                        result.Add(SingleDecode(buffer));
                    else if (buffer.Length == 8)
                        result.Add(SingleDecode(buffer));
                    else
                    {
                        if (buffer.Length > 8)
                        {
                            byte[] request = new byte[buffer.Length - 8];
                            byte[] response = new byte[8];
                            Array.Copy(buffer, request, buffer.Length - 8);
                            Array.Copy(buffer, buffer.Length - 8, response, 0, 8);
                            result.Add(SingleDecode(request));
                            List<ModbusPacket> rest = Decode(response);
                            if (rest != null)
                                result.AddRange(rest);
                        }
                        else
                            result.Add(SingleDecode(buffer));
                    }
                    break;
            }

            return result;
        }

        private static ModbusPacket SingleDecode(byte[] buffer)
        {
            ushort address = buffer[0];
            ushort function = buffer[1];

            switch (function)
            {
                case 0x01:
                    if (buffer.Length == 8 & buffer[2] != 3)
                    {
                        try
                        {
                            ReadCoilsRequest readCoilsReq = new ReadCoilsRequest();
                            readCoilsReq.PDU = buffer;
                            readCoilsReq.Slave = address;
                            readCoilsReq.Function = function;
                            readCoilsReq.StartingAddress = (buffer[2] << 8) + buffer[3];
                            readCoilsReq.Quantity = (buffer[4] << 8) + buffer[5];

                            return readCoilsReq;
                        }
                        catch { break; }
                    }
                    else
                    {
                        try
                        {
                            ReadCoilsResponse readCoilsRes = new ReadCoilsResponse();
                            readCoilsRes.PDU = buffer;
                            readCoilsRes.Slave = address;
                            readCoilsRes.Function = function;
                            readCoilsRes.ByteCount = buffer[2];

                            return readCoilsRes;
                        }
                        catch { break; }
                    }

                case 0x03:
                    if (buffer.Length == 8)
                    {
                        try
                        {
                            ReadRegistersRequest readRegsReq = new ReadRegistersRequest();
                            readRegsReq.PDU = buffer;
                            readRegsReq.Slave = address;
                            readRegsReq.Function = function;
                            readRegsReq.StartingAddress = (buffer[2] << 8) + buffer[3];
                            readRegsReq.Quantity = (buffer[4] << 8) + buffer[5];

                            return readRegsReq;
                        }
                        catch { break; }
                    }
                    else
                    {
                        try
                        {
                            ReadRegistersResponse readRegsRes = new ReadRegistersResponse();
                            readRegsRes.PDU = buffer;
                            readRegsRes.Slave = address;
                            readRegsRes.Function = function;
                            readRegsRes.ByteCount = buffer[2];

                            return readRegsRes;
                        }
                        catch { break; }
                    }

                case 0x05:
                    try
                    {
                        WriteSingleCoil writeSingleCoil = new WriteSingleCoil();
                        writeSingleCoil.PDU = buffer;
                        writeSingleCoil.Slave = address;
                        writeSingleCoil.Function = function;
                        writeSingleCoil.Register = (buffer[2] << 8) + buffer[3];
                        writeSingleCoil.Value = (buffer[4] << 8) + buffer[5] != 0;

                        return writeSingleCoil;
                    }
                    catch { break; }

                case 0x06:
                    try
                    {
                        WriteSingleRegister writeSingleReg = new WriteSingleRegister();
                        writeSingleReg.PDU = buffer;
                        writeSingleReg.Slave = address;
                        writeSingleReg.Function = function;
                        writeSingleReg.Register = (buffer[2] << 8) + buffer[3];
                        writeSingleReg.Value = (buffer[4] << 8) + buffer[5];

                        return writeSingleReg;
                    }
                    catch { break; }

                case 0x0F:
                    if (buffer.Length != 8)
                    {
                        try
                        {
                            WriteCoilsRequest writeCoilsReq = new WriteCoilsRequest();
                            writeCoilsReq.PDU = buffer;
                            writeCoilsReq.Slave = address;
                            writeCoilsReq.Function = function;
                            writeCoilsReq.StartingAddress = (buffer[2] << 8) + buffer[3];
                            writeCoilsReq.Quantity = (buffer[4] << 8) + buffer[5];
                            writeCoilsReq.ByteCount = buffer[6];

                            return writeCoilsReq;
                        }
                        catch { break; }
                    }
                    else
                    {
                        try
                        {
                            WriteCoilsResponse writeCoilsRes = new WriteCoilsResponse();
                            writeCoilsRes.PDU = buffer;
                            writeCoilsRes.Slave = address;
                            writeCoilsRes.Function = function;
                            writeCoilsRes.StartingAddress = (buffer[2] << 8) + buffer[3];
                            writeCoilsRes.Quantity = (buffer[4] << 8) + buffer[5];

                            return writeCoilsRes;
                        }
                        catch { break; }
                    }

                case 0x10:
                    if (buffer.Length != 8)
                    {
                        try
                        {
                            WriteRegistersRequest writeRegsReq = new WriteRegistersRequest();
                            writeRegsReq.PDU = buffer;
                            writeRegsReq.Slave = address;
                            writeRegsReq.Function = function;
                            writeRegsReq.StartingAddress = (buffer[2] << 8) + buffer[3];
                            writeRegsReq.Quantity = (buffer[4] << 8) + buffer[5];
                            writeRegsReq.ByteCount = buffer[6];

                            return writeRegsReq;
                        }
                        catch { break; }
                    }
                    else
                    {
                        try
                        {
                            WriteRegistersResponse writeRegsRes = new WriteRegistersResponse();
                            writeRegsRes.PDU = buffer;
                            writeRegsRes.Slave = address;
                            writeRegsRes.Function = function;
                            writeRegsRes.StartingAddress = (buffer[2] << 8) + buffer[3];
                            writeRegsRes.Quantity = (buffer[4] << 8) + buffer[5];

                            return writeRegsRes;
                        }
                        catch { break; }
                    }

                default:
                    if (function > 0x80)
                    {
                        try
                        {
                            ExceptionResponse excRes = new ExceptionResponse();
                            excRes.PDU = buffer;
                            excRes.Slave = address;
                            excRes.Function = (ushort)(function - 0x80);
                            excRes.ExceptionCode = buffer[2];

                            return excRes;
                        }
                        catch { break; }
                    }
                    else
                    {
                        ModbusPacket unsupported = new ModbusPacket();
                        unsupported.PDU = buffer;
                        unsupported.Slave = address;
                        unsupported.Function = function;

                        return unsupported;
                    }
            }

            ModbusPacket incomplete = new ModbusPacket();
            incomplete.PDU = buffer;
            incomplete.Slave = address;
            incomplete.Function = function;

            return incomplete;
        }
    }
}
