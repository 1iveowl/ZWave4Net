﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZWave4Net.Commands
{
    public class Basic : CommandClass
    {
        public event EventHandler<ValueChangedEventArgs<byte>> ValueChanged;

        enum command
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public Basic(Node node)
            : base(0x20, node)
        {
        }
        
        protected void OnValueChanged(ValueChangedEventArgs<byte> e)
        {
            var handler = ValueChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override Enum[] Commands
        {
            get { return Enum.GetValues(typeof(command)).Cast<Enum>().ToArray(); }
        }

        public async Task<byte> GetValue()
        {
            var response = await Invoker.Invoke(new Command(ClassID, command.Get));
            return response.Payload.First();
        }

        public Task SetValue(byte value)
        {
            return Invoker.Invoke(new Command(ClassID, command.Set, value));
        }

        protected override bool IsCorrelated(Enum request, Enum response)
        {
            return object.Equals(request, command.Get) && object.Equals(response, command.Report);
        }

        protected override void OnReport(Enum command, byte[] payload)
        {
            var value = payload.First();
            Platform.LogMessage(LogLevel.Debug, string.Format($"Event: Node = {Node}, Class = {ClassName}, Command = {command}, Value = {value}"));

            OnValueChanged(new ValueChangedEventArgs<byte>(value));
        }
    }
}