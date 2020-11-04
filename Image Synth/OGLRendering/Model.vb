Imports OpenTK
Imports OpenTK.Graphics.ES30

Public Class ModelInstance
    Property Name As String

    Property Location As New Vector3
    Property Rotation As New Vector3
    Property Scale As New Vector3(1)
    Property Spin As New Vector3

    Private cElementBufferItems As Int32

    Property VertexInfos As New List(Of VertexInfo)
    Property VertexBuffer As Single()
    Property ElementBuffer As UInt16()


    Public Structure VertexInfo
        Public Count As Int32
        Public Type As VertexAttribPointerType

        Public Sub New(newCount As Int32, newType As VertexAttribPointerType)
            Count = newCount
            Type = newType
        End Sub

        ReadOnly Property Size As Int32
            Get
                Dim cbResult As Int32
                Select Case Type
                    Case VertexAttribPointerType.Float
                        cbResult = Count * cbSingle
                    Case Else
                        Stop
                End Select
                Return cbResult
            End Get
        End Property
    End Structure



    Public Sub New()
        CreatePlane()
        'CreateCube()
    End Sub

    Private Sub CreatePlane()
        VertexBuffer = {-0.5, +0.0, +0.5, 0.0, 0.0,
                        -0.5, +0.0, -0.5, 0.0, 1.0,
                         0.5, +0.0, -0.5, 1.0, 1.0,
                         0.5, +0.0, +0.5, 1.0, 0.0
        }
        VertexInfos.AddRange({New VertexInfo(3, VertexAttribPointerType.Float),
                              New VertexInfo(2, VertexAttribPointerType.Float)})
        ElementBuffer = {0, 1, 2, 0, 2, 3}

    End Sub

    Private Sub CreateCube()
        VertexBuffer = {-0.5, -0.5, +0.5, 0.0, 0.0,
                        -0.5, -0.5, -0.5, 0.0, 1.0,
                         0.5, -0.5, -0.5, 1.0, 1.0,
                         0.5, -0.5, +0.5, 1.0, 0.0,
                        -0.5, +0.5, +0.5, 1.0, 0.0,
                        -0.5, +0.5, -0.5, 1.0, 1.0,
                         0.5, +0.5, -0.5, 0.0, 1.0,
                         0.5, +0.5, +0.5, 0.0, 0.0,
                         0.5, -0.5, +0.5, 0.0, 1.0,
                         0.5, +0.5, +0.5, 1.0, 1.0,
                        -0.5, +0.5, -0.5, 0.0, 0.0,
                         0.5, +0.5, -0.5, 1.0, 0.0
        }
        VertexInfos.AddRange({New VertexInfo(3, VertexAttribPointerType.Float),
                              New VertexInfo(2, VertexAttribPointerType.Float)})
        ElementBuffer = {0, 1, 2, 0, 2, 3,
                         7, 3, 2, 7, 2, 6,
                         7, 6, 5, 7, 5, 4,
                         4, 1, 0, 4, 5, 1,
                         4, 0, 8, 4, 8, 9,
                         10, 2, 1, 10, 11, 2}
        Me.Spin = New Vector3(0.01, 0.01, 0.01)
    End Sub


    Public Sub UpdateWorld()
        Rotation = Vector3.Add(Spin, Rotation)
    End Sub

    ReadOnly Property ModelMatrix() As Matrix4
        Get
            Dim R As Matrix4 = Matrix4.Mult(Matrix4.CreateRotationX(Rotation.X), Matrix4.CreateRotationZ(Rotation.Y))
            Matrix4.Mult(Matrix4.CreateRotationZ(Rotation.Z), R, R)

            Dim S As Matrix4 = Matrix4.CreateScale(Scale)
            Matrix4.Mult(R, S, R)
            Return Matrix4.Mult(R, Matrix4.CreateTranslation(Location.X, Location.Y, Location.Z))
        End Get
    End Property
End Class
