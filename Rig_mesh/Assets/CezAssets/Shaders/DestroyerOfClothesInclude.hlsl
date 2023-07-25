//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

float _DistanceToPlane;
float4 _Pos1;
float4 _Pos2;
float4 _Normal1;
float4 _Normal2;
float3 _PlaneNormal;
float _PlaneD;
float3 _Pos3;
float3 _Pos4;
float3 _Mid1;
float3 _Mid2;
float3 _MidDir;
float3 _MidPoint;
float _DistanceToMidline;
float _DistanceToMidlinePos2;
float3 UV;
float _distanceAlongMidlineFromMidPoint;
float3 d0;
float3 u;
float3 v;
float3 point1;
float3 point2;
float3 point3;
float3 point4;

bool checkIfPointInPolygon(float3 p0, float3 p1, float3 p2, float3 p3, float3 p){
        d0 = cross( p-p0 , p0-p3) ;
        if(dot(d0,cross( p-p1 , p1-p0 ))<=0.0) 
        //|| dot(d0,cross( p-p2 , p2-p1 ))<=0.0 || dot(d0,cross( p-p3 , p3-p2 ))<=0.0)
        return false;
        return true;
}


void DestroyerOfClothes_float(UnityTexture2D Pos1, UnityTexture2D Normal1, UnityTexture2D Pos2, UnityTexture2D Normal2, float Width, float3 ShaderPosition, UnitySamplerState pointSampler, out bool Clip)
{
    UV.x=0;
    UV.y=0;
    UV.z=0;
    Clip = false;
   for(int i=0;i<256;i++) { 
    UV.x=i;
    _Pos1= Pos1.Load(UV);
    if(_Pos1.w == 0) {} else {
    _Normal1=Normal1.Load(UV);
    _Pos2=Pos2.Load(UV);
    _Normal2=Normal2.Load(UV);
   _PlaneNormal = cross(_Normal1.xyz, _Pos1.xyz-_Pos2.xyz);
   _PlaneD=-(_Pos1.x * _PlaneNormal.x + _Pos1.y * _PlaneNormal.y + _Pos1.z * _PlaneNormal.z);
   
   _DistanceToPlane =abs(ShaderPosition.x * _PlaneNormal.x + ShaderPosition.y * _PlaneNormal.y + ShaderPosition.z * _PlaneNormal.z + _PlaneD)/sqrt(_PlaneNormal.x * _PlaneNormal.x + _PlaneNormal.y * _PlaneNormal.y + _PlaneNormal.z * _PlaneNormal.z);
   //Normal.w is side length here
    _Mid1 = (_Pos1.xyz+_Pos2.xyz)/2;
   _Pos3 = _Pos1.xyz + _Normal1.xyz * _Normal1.w;
   _Pos4 = _Pos2.xyz + _Normal2.xyz * _Normal2.w;
   _Mid2 = (_Pos3+_Pos4)/2;
   _MidDir = _Mid2-_Mid1;
   _MidPoint= (_Mid1+_Mid2)/2;


   _DistanceToMidline = length(cross(ShaderPosition-_Mid1, _MidDir))/length(_MidDir);
   _DistanceToMidlinePos2 = length(cross(_Pos2.xyz-_Mid1, _MidDir))/length(_MidDir);
   _distanceAlongMidlineFromMidPoint = length (dot(ShaderPosition - _MidPoint, normalize(_MidDir)));
   


   if (_DistanceToPlane <= sqrt((_DistanceToMidlinePos2-_DistanceToMidline))/10    
   &&  length(dot(ShaderPosition-_MidPoint, normalize(_MidDir)))<=length(dot(_Mid1-_MidPoint, normalize(_MidDir)))
   ) Clip=true;
   // && checkIfPointInPolygon(_Pos1,_Pos2,_Pos3,_Pos4, ShaderPosition-cross(ShaderPosition-_MidPoint, normalize(_PlaneNormal)))) Clip = true;
   //&&  _distanceAlongMidlineFromMidPoint<length(_MidDir)
   }//&& length(ShaderPosition- _MidPoint) - max(length(_Pos1 - _MidPoint),length(_Pos2- _MidPoint))
}

}
#endif
