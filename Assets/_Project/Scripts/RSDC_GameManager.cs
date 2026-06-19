using UnityEngine;
using nakatimat.Core;

namespace nakatimat.RSDC
{
    public class RSDC_GameManager : GameManager
    {
        protected override void Start()
        {
            base.Start();

            Debug.LogWarning("O Mouse Sumiu!  Boa  Sorte tentando clicar  bobão!");
        }
    }
}