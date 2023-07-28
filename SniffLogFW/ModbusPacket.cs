namespace SniffLogFW
{
    public class ModbusPacket
    {
        public static string[] Exceptions = new string[]
        {
            "UNDEFINED",
            "Illegal function",
            "Illegal data address",
            "Illegal data value",
            "Slave device failure",
            "Acknowledge",
            "Slave device busy",
            "UNDEFINED",
            "Memory parity error",
        };

        public ushort Slave { get; set; }
        public ushort Function { get; set; }

        public byte[] PDU;

        public ModbusPacket()
        { }

        public string Message()
        {
            string result = "";

            for (int i = 0; i < PDU.Length; i++)
                result += string.Format("{0} ", PDU[i].ToString("X2"));

            return result;
        }

        public override string ToString()
        {
            return string.Format("Indefinita: slave {0}, funzione {1}", Slave, Function);
        }

        public bool Contains(int slave)
        {
            if (Slave == slave)
                return true;
            return false;
        }

        public bool Contains(int slave, int function)
        {
            if (slave == -1)
            {
                if (function == Function)
                    return true;
                else
                    return false;
            }

            if (function == -1)
            {
                if (slave == Slave)
                    return true;
                else
                    return false;
            }

            if ((slave == Slave) & (function == Function))
                return true;

            return false;
        }
    }

    public class ReadRegistersRequest : ModbusPacket
    {
        public int StartingAddress { get; set; }
        public int Quantity { get; set; }

        public ReadRegistersRequest()
        { }

        public override string ToString()
        {
            return string.Format("Richiesta di lettura multipla: slave {0}, funzione {1}, da registro {2} a {3}", Slave, Function, StartingAddress, StartingAddress + Quantity - 1);
        }
    }

    public class ReadRegistersResponse : ModbusPacket
    {
        public int ByteCount { get; set; }
        public int DataCount { get { return ByteCount * 2; } }

        public ReadRegistersResponse()
        { }

        public override string ToString()
        {
            return string.Format("Risposta di lettura multipla: slave {0}, funzione {1}, {2} registri", Slave, Function, DataCount);
        }
    }

    public class WriteSingleRegister : ModbusPacket
    {
        public int Register { get; set; }
        public int Value { get; set; }

        public WriteSingleRegister()
        { }

        public override string ToString()
        {
            return string.Format("Richiesta o risposta di scrittura singola: slave {0}, funzione {1}, registro {2} con valore {3}", Slave, Function, Register, Value);
        }
    }

    public class WriteRegistersRequest : ModbusPacket
    {
        public int StartingAddress { get; set; }
        public int Quantity { get; set; }
        public int ByteCount { get; set; }

        public WriteRegistersRequest()
        { }

        public override string ToString()
        {
            return string.Format("Richiesta di scrittura multipla: slave {0}, funzione {1}, da registro {2} a {3}", Slave, Function, StartingAddress, StartingAddress + Quantity - 1);
        }
    }

    public class WriteRegistersResponse : ModbusPacket
    {
        public int StartingAddress { get; set; }
        public int Quantity { get; set; }

        public WriteRegistersResponse()
        { }

        public override string ToString()
        {
            return string.Format("Risposta di scrittura multipla: slave {0}, funzione {1}, da registro {2} a {3}", Slave, Function, StartingAddress, StartingAddress + Quantity - 1);
        }
    }

    public class ReadCoilsRequest : ModbusPacket
    {
        public int StartingAddress { get; set; }
        public int Quantity { get; set; }

        public ReadCoilsRequest()
        { }

        public override string ToString()
        {
            return string.Format("Richiesta di lettura multipla: slave {0}, funzione {1}, da coil {2} a {3}", Slave, Function, StartingAddress, StartingAddress + Quantity - 1);
        }
    }

    public class ReadCoilsResponse : ModbusPacket
    {
        public int ByteCount { get; set; }
        public int DataCount { get { return ByteCount * 2; } }

        public ReadCoilsResponse()
        { }

        public override string ToString()
        {
            return string.Format("Risposta di lettura multipla: slave {0}, funzione {1}, {2} coil", Slave, Function, DataCount);
        }
    }

    public class WriteSingleCoil : ModbusPacket
    {
        public int Register { get; set; }
        public bool Value { get; set; }

        public WriteSingleCoil()
        { }

        public override string ToString()
        {
            return string.Format("Richiesta o risposta di scrittura singola: slave {0}, funzione {1}, coil {2} con valore {3}", Slave, Function, Register, Value);
        }
    }

    public class WriteCoilsRequest : ModbusPacket
    {
        public int StartingAddress { get; set; }
        public int Quantity { get; set; }
        public int ByteCount { get; set; }

        public WriteCoilsRequest()
        { }

        public override string ToString()
        {
            return string.Format("Richiesta di scrittura multipla: slave {0}, funzione {1}, da coil {2} a {3}", Slave, Function, StartingAddress, StartingAddress + Quantity - 1);
        }

    }

    public class WriteCoilsResponse : ModbusPacket
    {
        public int StartingAddress { get; set; }
        public int Quantity { get; set; }

        public WriteCoilsResponse()
        { }

        public override string ToString()
        {
            return string.Format("Risposta di scrittura multipla: slave {0}, funzione {1}, da coil {2} a {3}", Slave, Function, StartingAddress, StartingAddress + Quantity - 1);
        }

    }

    public class ExceptionResponse : ModbusPacket
    {
        public ushort ExceptionCode { get; set; }

        public ExceptionResponse()
        { }

        public override string ToString()
        {
            return string.Format("Eccezione: slave {0}, funzione {1} -> {2}", Slave, Function, Exceptions[ExceptionCode]);
        }
    }
}
