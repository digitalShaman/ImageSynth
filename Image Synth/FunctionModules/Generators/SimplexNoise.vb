'based on
'http://staffwww.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf

Imports System.Globalization

Public Class SimplexNoise
    Inherits FunctionBase

    Private fIntensity As Double = 0.5
    Private fAdd As Double = 1

    Private xScale As Double = 10
    Private yScale As Double = 10
    Private xShift As Double = 0
    Private yShift As Double = 0

    Public Overrides Function Evaluate(ByVal x As Double, ByVal y As Double) As Double
        Return (noise((x - 0.5) * xScale + xShift, (y - 0.5) * yScale + yShift) + fAdd) * fIntensity
    End Function

    Public Overrides ReadOnly Property Name As String
        Get
            Return ("Simplex Noise")
        End Get
    End Property

    Public Overrides ReadOnly Property Help As String
        Get
            Return String.Format("Generiert ein 2d Simplex Noise Signal gemäß dem Code von hier:{0}" &
                                 "http://staffwww.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf {0}" &
                                "{0}" &
                "Result = (({0}" &
                "   SimplexNoise(x * xScale + xShift, {0}" &
                "                y * yScale + yShift) {0}" &
                ") + Add ) * Intensity {0}" &
                "", Environment.NewLine)
        End Get
    End Property


    Public Overrides ReadOnly Property Type As FunctionType
        Get
            Return FunctionType.Generator
        End Get
    End Property


    Public Overrides ReadOnly Property ParameterInfos As System.Data.DataTable
        Get
            Dim tbl As New DataTable
            tbl.Columns.Add("Index", GetType(Int32))
            tbl.Columns.Add("Name", GetType(String))
            tbl.Columns.Add("Step", GetType(Double))
            tbl.Columns.Add("Default", GetType(Double))
            tbl.Columns.Add("Current", GetType(Double))
            tbl.Rows.Add(0, "Add", 0.1, 1, fAdd)
            tbl.Rows.Add(1, "Intensity", 0.1, 0.5, fIntensity)
            tbl.Rows.Add(2, "Scale X", 0.1, 10, xScale)
            tbl.Rows.Add(3, "Scale Y", 0.1, 10, yScale)
            tbl.Rows.Add(4, "Location X", 0.1, 0, xShift)
            tbl.Rows.Add(5, "Location Y", 0.1, 0, yShift)
            Return tbl
        End Get
    End Property



    Public Overrides Sub SetParameterValue(ByVal Index As Integer, ByVal Value As Double)
        Select Case Index
            Case 0
                fAdd = Value
            Case 1
                fIntensity = Value
            Case 2
                xScale = Value
            Case 3
                yScale = Value
            Case 4
                xShift = Value
            Case 5
                yShift = Value
        End Select
    End Sub

    Public Overrides Sub AddParameterValue(Index As Integer, Value As Double)
        Select Case Index
            Case 0
                fAdd += Value
            Case 1
                fIntensity += Value
            Case 2
                xScale += Value
            Case 3
                yScale += Value
            Case 4
                xShift += Value
            Case 5
                yShift += Value
        End Select
    End Sub

    Private grad3 As Int32()() = {New Int32() {1, 1, 0}, New Int32() {-1, 1, 0}, New Int32() {1, -1, 0}, New Int32() {-1, -1, 0},
                                  New Int32() {1, 0, 1}, New Int32() {-1, 0, 1}, New Int32() {1, 0, -1}, New Int32() {-1, 0, -1},
                                  New Int32() {0, 1, 1}, New Int32() {0, -1, 1}, New Int32() {0, 1, -1}, New Int32() {0, -1, -1}}

    'To remove the need for index wrapping, double the permutation table length
    Private perm As Int32() = {
            151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148,
            247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
            74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54,
            65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64,
            52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
            119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104,
            218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
            184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180,
            151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148,
            247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
            74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54,
            65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64,
            52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
            119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104,
            218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
            184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
        }



    Private Function dot(ByVal g As Int32(), ByVal x As Double, ByVal y As Double) As Double
        Return g(0) * x + g(1) * y
    End Function


    ' 2D simplex noise
    Public Function noise(ByVal xin As Double, ByVal yin As Double) As Double
        Dim n0, n1, n2 As Double ' Noise contributions from the three corners

        ' Skew the input space to determine which simplex cell we're in
        Static F2 As Double = 0.5 * (Math.Sqrt(3.0) - 1.0)
        Dim s As Double = (xin + yin) * F2 ' Hairy factor for 2D
        Dim i As Int32 = CInt(Math.Floor(xin + s))
        Dim j As Int32 = CInt(Math.Floor(yin + s))

        Static G2 As Double = (3.0 - Math.Sqrt(3.0)) / 6.0
        Dim t As Double = (i + j) * G2
        Dim X0 As Double = i - t ' Unskew the cell origin back to (x,y) space
        Dim Y0 As Double = j - t
        Dim x0O As Double = xin - X0 ' The x,y distances from the cell origin
        Dim y0O As Double = yin - Y0

        ' For the 2D case, the simplex shape is an equilateral triangle.
        ' Determine which simplex we are in.
        Dim i1, j1 As Int32 ' Offsets for second (middle) corner of simplex in (i,j) coords
        If (x0O > y0O) Then
            i1 = 1 ' lower triangle, XY order: (0,0)->(1,0)->(1,1)
            j1 = 0
        Else
            i1 = 0  ' upper triangle, YX order: (0,0)->(0,1)->(1,1)
            j1 = 1
        End If

        ' A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
        ' a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
        ' c = (3-sqrt(3))/6

        Dim x1 As Double = x0O - i1 + G2 ' Offsets for middle corner in (x,y) unskewed coords
        Dim y1 As Double = y0O - j1 + G2
        Dim x2 As Double = x0O - 1.0 + 2.0 * G2 ' Offsets for last corner in (x,y) unskewed coords
        Dim y2 As Double = y0O - 1.0 + 2.0 * G2

        ' Work out the hashed gradient indices of the three simplex corners
        Dim ii As Int32 = i And 255
        Dim jj As Int32 = j And 255
        Dim gi0 As Int32 = perm(ii + perm(jj)) Mod 12

        Dim gi1 As Int32 = perm(ii + i1 + perm(jj + j1)) Mod 12
        Dim gi2 As Int32 = perm(ii + 1 + perm(jj + 1)) Mod 12

        ' Calculate the contribution from the three corners
        Dim t0 As Double = 0.5 - x0O * x0O - y0O * y0O

        If (t0 < 0) Then
            n0 = 0.0
        Else
            t0 *= t0
            n0 = t0 * t0 * dot(grad3(gi0), x0O, y0O) ' (x,y) of grad3 used for 2D gradient
        End If


        Dim t1 As Double = 0.5 - x1 * x1 - y1 * y1
        If (t1 < 0) Then
            n1 = 0.0
        Else
            t1 *= t1
            n1 = t1 * t1 * dot(grad3(gi1), x1, y1)
        End If

        Dim t2 As Double = 0.5 - x2 * x2 - y2 * y2
        If (t2 < 0) Then
            n2 = 0.0
        Else
            t2 *= t2
            n2 = t2 * t2 * dot(grad3(gi2), x2, y2)
        End If

        ' Add contributions from each corner to get the final noise value.
        ' The result is scaled to return values in the interval [-1,1].
        Return 70.0 * (n0 + n1 + n2)
    End Function









    Overrides Sub AddOpenGlCode(ByRef DeclareCode As String, ByRef InitCode As String, ByRef ExecuteCode As String, AnySolo As Boolean)
        If Not DeclareCode.Contains("float EvaluateSimplexNoise(int iModule, vec2 Location)") Then
            DeclareCode &= GetDeclareCode()
        End If
        InitCode &= GetInitCode()
        ExecuteCode &= GetExecuteCode(AnySolo)
    End Sub

    Private Function GetDeclareCode() As String
        Return "
const ivec3 grad3[12] = ivec3[12](ivec3(1, 1, 0), ivec3(-1,  1, 0), ivec3(1, -1,  0), ivec3(-1, -1,  0),
								  ivec3(1, 0, 1), ivec3(-1,  0, 1), ivec3(1,  0, -1), ivec3(-1,  0, -1),
								  ivec3(0, 1, 1), ivec3( 0, -1, 1), ivec3(0,  1, -1), ivec3( 0, -1, -1));

const int perm[512] = int[512](
		 151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148
		,247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32, 57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175
		, 74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122, 60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54
		, 65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64
		, 52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213
		,119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104
		,218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241, 81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157
		,184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
		,151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148
		,247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32, 57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175
		, 74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122, 60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54
		, 65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64
		, 52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213
		,119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104
		,218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241, 81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157
		,184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
);

const float F2 = 0.5 * (sqrt(3.0) - 1.0);
const float G2 = (3.0 - sqrt(3.0)) / 6.0;


float EvaluateSimplexNoise(int iModule, vec2 Location) {
	Location.x = (Location.x - 0.5) * Storage[iModule + 2] + Storage[iModule + 4];
	Location.y = (Location.y - 0.5) * Storage[iModule + 3] + Storage[iModule + 5];

	float n0, n1, n2;

	float s = (Location.x + Location.y) * F2;	// Hairy factor for 2D
	float i = floor(Location.x + s);
	float j = floor(Location.y + s);

	float t = (i + j) * G2;
    float X0 = i - t;			/* Unskew the cell origin back to (x,y) space */
    float Y0 = j - t;
    float x0O = Location.x - X0; /* The x,y distances from the cell origin */
    float y0O = Location.y - Y0;


	/* For the 2D case, the simplex shape is an equilateral triangle.
	   Determine which simplex we are in. */
	int i1, j1;		/* Offsets for second (middle) corner of simplex in (i,j) coords */
	if(x0O > y0O) {
		i1 = 1;	j1 = 0;	/* lower triangle, XY order: (0,0)->(1,0)->(1,1) */
	} else {
		i1 = 0;	j1 = 1;	/* upper triangle, YX order: (0,0)->(0,1)->(1,1) */
	}

	float x1 = x0O - float(i1) + G2;	/* Offsets for middle corner in (x,y) unskewed coords */
    float y1 = y0O - float(j1) + G2;
	float x2 = x0O - 1.0 + 2.0 * G2;	/* Offsets for last corner in (x,y) unskewed coords */
    float y2 = y0O - 1.0 + 2.0 * G2;

	/* Work out the hashed gradient indices of the three simplex corners */
    int ii = int(i) & 255;
	int jj = int(j) & 255;
	int gi0 = int(mod(float(perm[ii + perm[jj]]), 12.0));

	int gi1 = int(mod( float(perm[ii + i1 + perm[jj + j1]]), 12.0 ));
    int gi2 = int(mod( float(perm[ii + 1 + perm[jj + 1]]), 12.0));

	/* Calculate the contribution from the three corners */
    float t0 = 0.5 - x0O * x0O - y0O * y0O;

    if (t0 < 0.0) {
		n0 = 0.0;
	} else {
        t0 *= t0;
		n0 = t0 * t0 * dot(vec2(grad3[gi0].xy), vec2(x0O, y0O));	/* (x,y) of grad3 used for 2D gradient */
	}

	float t1 = 0.5 - x1 * x1 - y1 * y1;

	if (t1 < 0.0) {
		n1 = 0.0;
	} else {
        t1 *= t1;
        n1 = t1 * t1 * dot(vec2(grad3[gi1].xy), vec2(x1, y1));
	}

	float t2 = 0.5 - x2 * x2 - y2 * y2;
	if (t2 < 0.0) {
		n2 = 0.0;
	} else {
		t2 *= t2;
		n2 = t2 * t2 * dot(vec2(grad3[gi2].xy), vec2(x2, y2));
	}


	float fResult = 70.0 * (n0 + n1 + n2);
    return (fResult + Storage[iModule + 0]) * Storage[iModule + 1];
}"
    End Function

    Private Function GetInitCode() As String
        Dim sb As New Text.StringBuilder
        Dim sFormat As String = "0.0######"
        sb.Append($"Storage[{OGLStorageIndex + 0}]={fAdd.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 1}]={fIntensity.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 2}]={xScale.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 3}]={yScale.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 4}]={xShift.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 5}]={yShift.ToString(sFormat, CultureInfo.InvariantCulture)};")

        Return sb.ToString
    End Function

    Private Function GetExecuteCode(AnySolo As Boolean) As String
        Dim sTempVarName As String = $"value{OGLStorageIndex}"
        Dim sExecuteCode As String = $"float {sTempVarName}=EvaluateSimplexNoise({OGLStorageIndex}, textureCoordinate);"

        'Ergebnis an folge funktionen weitergeben
        For Each oChild As OutputTo In Outputs
            If Not (AnySolo AndAlso oChild.ToFunction.Type = FunctionType.ImageOutput) AndAlso oChild.ToFunction.OGLStorageIndex <> -1 Then
                'aber nur, wenn kein solo oder wenn solo dann aber nicht an imageoutput
                Dim oFunc As FunctionBase = CType(oChild.ToFunction, FunctionBase)
                sExecuteCode &= oFunc.GetOpenGlAddCode(oChild.ToPin, sTempVarName)
            End If
        Next

        Return sExecuteCode
    End Function


End Class
