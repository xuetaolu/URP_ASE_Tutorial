
using UnityEngine;

[ExecuteAlways] //在Editor模式下&GAME模式下都会执行生效
public class HeadTowards : MonoBehaviour
{
    public Material FaceMaterial;
    //挂载此脚本到模型头部骨骼下 同时添加上面部材质球
    private void SetHeadDirection()
    {
        if (this.FaceMaterial !=null) //检查面部材质是否为空
        {
            this. FaceMaterial.SetVector("_HeadForward", this.transform.forward); //将当前对象的Transform Forward传递给Shader材质球里的HeadForward
            this. FaceMaterial.SetVector("_HeadRight", this.transform.right); //将当前对象的Transform Right传递给Shader材质球里的HeadRight
        }
    }


    private void Update()
    {
        this.SetHeadDirection();//update方法中执行此函数
    }
}
