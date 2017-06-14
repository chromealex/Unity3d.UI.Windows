using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// The behaviour of this class is almost the same as the original except:
/// 1. It absorbs version differences.
/// 2. It corrects the calculation of vertex list capacity.
/// </summary>
public class ModifiedShadow : Shadow
{
    protected new void ApplyShadow(List<UIVertex> verts, Color32 color, int start, int end, float x, float y)
    {
        UIVertex vt;

        // The capacity calculation of the original version seems wrong.
        var neededCpacity = verts.Count + (end - start);
        if (verts.Capacity < neededCpacity)
            verts.Capacity = neededCpacity;

        for (int i = start; i < end; ++i)
        {
            vt = verts[i];
            verts.Add(vt);

            Vector3 v = vt.position;
            v.x += x;
            v.y += y;
            vt.position = v;
            var newColor = color;
            if (useGraphicAlpha)
                newColor.a = (byte)((newColor.a * verts[i].color.a) / 255);
            vt.color = newColor;
            verts[i] = vt;
        }
    }

#if UNITY_5_2 && !UNITY_5_2_1pX
    public override void ModifyMesh(Mesh mesh)
    {
        if (!this.IsActive())
            return;

        using (var vh = new VertexHelper(mesh))
        {
            ModifyMesh(vh);
            vh.FillMesh(mesh);
        }
    }
#endif

#if !(UNITY_4_6 || UNITY_5_0 || UNITY_5_1)
#if UNITY_5_2_1pX || UNITY_5_3
    public override void ModifyMesh(VertexHelper vh)
#else
    public override void ModifyMesh(VertexHelper vh)
#endif
    {
        if (!this.IsActive())
            return;
        
        List<UIVertex> list = new List<UIVertex>();
        vh.GetUIVertexStream(list);
        
        ModifyVertices(list);

#if UNITY_5_2_1pX || UNITY_5_3
        vh.Clear();
#endif
        vh.AddUIVertexTriangleStream(list);
    }

    public virtual void ModifyVertices(List<UIVertex> verts)
    {
    }
#endif
}
