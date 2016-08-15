
Imports System
Imports System.IO
Imports System.Data
Imports System.Text
Imports System.Collections

Public Class Utils

    Public Sub New()

    End Sub

    '/ <summary>
    '/ Returns true for a precision 
    '/ </summary>
    '/ <param name="s"></param>
    '/ <param name="t"></param>
    '/ <returns></returns>
    Public Shared Function IsApproxEqual(ByVal s As Double, ByVal t As Double, ByVal precision As Double) As Boolean
        If Math.Abs(s - t) < precision Then
            Return True
        Else
            Return False
        End If
    End Function

    'append an extension to a filename removing the existing extension if there is one
    Shared Function ChangeFileExt(ByVal filenameIn As String, ByVal extIn As String) As String
        ChangeFileExt = filenameIn
        Dim i As Integer = ChangeFileExt.LastIndexOf(".")
        If (i >= 0) Then
            ChangeFileExt = ChangeFileExt.Remove(i, ChangeFileExt.Length() - i) 'strip extension
        End If
        ChangeFileExt &= "."
        ChangeFileExt &= extIn
        'MessageBox.Show("ChangeFileExt - exit - ChangeFileExt <" & ChangeFileExt & "> i <" & i & ">")
    End Function


    '  Helper function - recursively search the given file name under the current directory. 
    '
    Public Shared Function SearchFile(ByVal path As String, ByVal fileName As String) As String

        '  search this directory 
        Dim fname As String
        For Each fname In Directory.GetFiles(path, fileName) ', SearchOption.AllDirectories)
            'The above overload searches recursively
            Return path
        Next
        '  recursively search child directories.  
        Dim dname As String
        For Each dname In Directory.GetDirectories(path)
            If Not dname.Contains("Structural") Then
                Continue For
            End If
            Dim filePath As String = SearchFile(dname, fileName)
            If Not (filePath Is Nothing) Then
                Return filePath
            End If
        Next

        Return Nothing
    End Function
    Public Shared Function TrimSpace(ByVal source As String) As String
        Dim sReturn As String = ""
        Dim arr As String() = source.Split(New Char() {" "c}, System.StringSplitOptions.RemoveEmptyEntries)
        For Each str As String In arr
            sReturn = sReturn + str
        Next
        Return sReturn
    End Function
    'Public Shared Function RegisterOasysFamilies(ByRef revitApp As Revit.Application, ByVal dllPath As String) As Boolean

    '    Dim paths As Revit.Collections.StringStringMap = revitApp.Options.LibraryPaths
    '    Debug.Assert(Not paths.Contains("Oasys Families"))

    '    Dim network_path As String = "", familyPath As String = ""

    '    ' Try the Path to Arup CADTools folder where the oasys families have been put
    '    network_path = "\\CADtools\CADTools\CADtools_Revit\Revit 2009 Combined Content\Oasys\Structural"
    '    If System.IO.Directory.Exists(network_path) Then
    '        familyPath = network_path
    '    Else
    '        dllPath = System.Environment.GetEnvironmentVariable("ALLUSERSPROFILE")
    '        familyPath = dllPath + "\Application data\Oasys\RST 2009\Structural"
    '    End If

    '    'dllPath = dllPath.Substring(0, dllPath.LastIndexOf("\"))
    '    Try
    '        Dim bCreationSuccess As Boolean = False
    '        If Not System.IO.Directory.Exists(familyPath) Then
    '            bCreationSuccess = Utils.CreateDirectory(familyPath)
    '        End If
    '        If Not bCreationSuccess Then
    '            Return False
    '        End If
    '        paths.Insert("Oasys Families", familyPath)
    '        revitApp.Options.LibraryPaths = paths
    '        Return True
    '    Catch ex As Exception
    '        Return False
    '    End Try

    'End Function
    Public Shared Function CreateDirectory(ByVal path As String) As Boolean

        Dim info As System.IO.DirectoryInfo = Nothing
        Try
            info = System.IO.Directory.CreateDirectory(path)
            If True = info.Exists Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Return False
        End Try
    End Function

End Class
