Imports System.Runtime.InteropServices
Imports OpenTK

Module modShared
    Public cbSingle As Int32 = Marshal.SizeOf(Of Single)
    Public cbUShort As Int32 = Marshal.SizeOf(Of UShort)

    Public Gamerotation As Vector3

    Public Function GetGameRotation() As Matrix4
        Dim rot As Matrix4 = Matrix4.CreateRotationX(Gamerotation.X)
        Return Matrix4.Mult(Matrix4.CreateRotationZ(Gamerotation.Z), rot)
    End Function

    Public Sub ResetGameRotation()
        Gamerotation = New Vector3
    End Sub
End Module
