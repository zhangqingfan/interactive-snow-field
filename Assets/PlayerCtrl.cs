using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    public GameObject snowPlane;
    private Material snowPlaneMat;
    
    public RenderTexture trackTexture;
   
    public Texture2D footprintTexture;
    [Range(0.1f, 10f)]
    public float footprintScale = 0.1f;

    private Vector3 lastPos;
    private Vector2? lastMousePos = null;

    void Start()
    {
        trackTexture.Release();
        snowPlaneMat = snowPlane.GetComponent<MeshRenderer>().sharedMaterial;
        lastPos = transform.position;
        DrawPostion(transform.position);
    }

    void Update()
    {
        PlayerMove();

        if (transform.position != lastPos)
        {
            lastPos = transform.position;
            DrawPostion(transform.position);
        }   
    }


    Vector2 bottomLeftPos = Vector2.zero;
    Vector2 topRightPos = Vector2.zero;
    void DrawPostion(Vector3 pos)
    {
        var ray = new Ray(pos, Vector3.down);
        RaycastHit hitInfo;
        
        if (Physics.Raycast(ray, out hitInfo, 100) == false)
            return;
        
        var footprintX = hitInfo.textureCoord.x * trackTexture.width;
        var footprintY = ( 1- hitInfo.textureCoord.y) * trackTexture.height;   //TODO...BUG!!!!!!!!
        //Debug.Log(hitInfo.textureCoord);

        var footprintWidth = footprintTexture.width * footprintScale;
        var footprintHeigh = footprintTexture.height * footprintScale;
        
        bottomLeftPos.x = footprintX - footprintWidth / 2;
        bottomLeftPos.y = footprintY - footprintHeigh / 2;
        
        topRightPos.x = footprintX + footprintWidth / 2;
        topRightPos.y = footprintY + footprintHeigh / 2;

        if (bottomLeftPos.x >= trackTexture.width || bottomLeftPos.y >= trackTexture.height)
            return;

        if(bottomLeftPos.x < 0)
        {
            footprintWidth += bottomLeftPos.x;
            bottomLeftPos.x = 0;
        }

        if(bottomLeftPos.y < 0)
        {
            footprintHeigh += bottomLeftPos.y;
            bottomLeftPos.y = 0;
        }
        
        if(topRightPos.x > trackTexture.width)
        {
            footprintWidth -= (topRightPos.x - trackTexture.width);
        }

        if(topRightPos.y > trackTexture.height)
        {
            footprintHeigh -= (topRightPos.y - trackTexture.height);
        }

        var oldRT = RenderTexture.active;
        RenderTexture.active = trackTexture;

        GL.PushMatrix();

        GL.LoadPixelMatrix(0, trackTexture.width, trackTexture.height, 0);
        //GL.LoadPixelMatrix(0, trackTexture.width, 0, trackTexture.height);
        var footprintUVRect = new Rect(0, 0, footprintWidth / (footprintTexture.width * footprintScale), footprintHeigh / (footprintTexture.height * footprintScale));  //0~1
        var trackRTRect = new Rect(bottomLeftPos.x, bottomLeftPos.y, footprintWidth, footprintHeigh);   
        Graphics.DrawTexture(trackRTRect, footprintTexture, footprintUVRect, 0, 0, 0, 0);

        GL.PopMatrix();
        
        RenderTexture.active = oldRT;
    }

    void PlayerMove()
    {
        if (Input.GetMouseButton(1) == true)
        {
            if(lastMousePos != null)
            {
                var lastPos = (Vector2)lastMousePos;
                var mouseOffset = new Vector2(Input.mousePosition.x - lastPos.x, Input.mousePosition.y - lastPos.y);
                var rotation = Quaternion.Euler(0, mouseOffset.x * 0.3f, 0);

                transform.rotation *= rotation;
            }

            lastMousePos = Input.mousePosition;
        }
        else
        {
            lastMousePos = null;
        }

        var axis = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if(Mathf.Abs(axis.x) >= 0.01f || Mathf.Abs(axis.z) >= 0.01f)
        {
            var dir = transform.rotation * axis.normalized;
            var distance = dir * Time.deltaTime * 20f;
            transform.Translate(distance, Space.World);
        }
    }
}
