using System;
using System.Collections.Generic;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Util;
using TecnicalKatarina.Configurations;

namespace TecnicalKatarina
{
    public class DaggerManager
    {
        public List<Vector3> AllDaggers { get; }
        
        public DaggerManager()
        {
            GameObject.OnCreate += OnCreate;
            Render.OnPresent += DrawThisShit;
            AllDaggers = new List<Vector3>();
        }

        private void DrawThisShit()
        {
            if (!Storings.MenuConfiguration.DrawDagger.Value)
            {
                return;
            }
            foreach (Vector3 dagger in AllDaggers)
            {
                Render.Circle(dagger, 140, 60, System.Drawing.Color.Aqua);
            }
        }
        
        private void OnCreate(GameObject sender)
        {
            
            if (Storings.QCreationObjects.Contains(sender.Name))
            {
                Vector3 senderPosition = sender.Position;
                AllDaggers.Add(senderPosition);
                DelayAction.Queue(Storings.DAGGERMAXACTIVETIME, () =>
                {
                    AllDaggers.RemoveAll(v => v == senderPosition);
                });
            }
            if (sender.Name.Contains(Storings.QDELETION))
            {
                Vector3 minDagger = AllDaggers.MinBy(d => sender.Position.Distance(d));
                if (minDagger.Distance(sender.Position) < 200)
                {
                    AllDaggers.Remove(minDagger);
                }
            }
        }
    }
}