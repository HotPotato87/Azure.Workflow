using System;
using System.IO;
using Servershot.Framework.Entities.WebJob;

namespace ServerShot.Framework.Core.Plugins
{
    public class JibJobLogger : WebJobSessionPluginBase
    {
        private readonly TextWriter _webJobTextWriter;

        public JibJobLogger(TextWriter webJobTextWriter)
        {
            _webJobTextWriter = webJobTextWriter;
        }

        public override void RegisterWebjob(IJibJobModule module)
        {
            base.RegisterWebjob(module);

            module.OnLogMessage += message => _webJobTextWriter.WriteLine("{0} : {1}", DateTime.Now, message);
        }
    }
}