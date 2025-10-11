using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WholePie0 : WholePie
{
    [SerializeField] List<WholePie> adjecentPies = new List<WholePie>();

    void Start()
    {
        adjecentPies[1].ClearAllSlices();
        adjecentPies[2].ClearAllSlices();
    }
}
