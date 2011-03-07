////////////////////////////////////////////////////////////////////////////////
//
// JavaScript library to load any .NET assembly/type and execute against its 
// public methods and properties
//
////////////////////////////////////////////////////////////////////////////////
var tfolder;
function GadgetBuilder()
{
    // ProgID of the Gadget Adapter for use in creating an ActiveX object instance
    var progID = "GadgetInterop.GadgetAdapter";
    // File name of the Gadget Interop DLL
    var assemblyName = "Gadget.Interop.dll";
    // Gadget Adapter class GUID
    var guid = "{89BB4535-5AE9-43a0-89C5-19B4697E5C5E}";
    // Gadget Adapter assembly location (under the gadget's root directory)
    var assemblyStore = "\\en-US\\bin\\";
    
    // Mudando os diretorios
	var fso = new ActiveXObject("Scripting.FileSystemObject");
	var TemporaryFolder = 2;
	tfolder = fso.GetSpecialFolder(TemporaryFolder)+ "\\Gadget\\";
	try
	{
	    var destFolder = tfolder;
	    if (!fso.FolderExists(destFolder))
            fso.CreateFolder(destFolder)

	    var sourceFolder = "" + System.Gadget.path + assemblyStore;
		var folder = fso.GetFolder(sourceFolder);
        var files = new Enumerator(folder.files)
        
		var s = "";
        for (; !files.atEnd(); files.moveNext())
        {
            try
            {
                s = files.item().name;
                fso.CopyFile(sourceFolder + s, destFolder + s);
            }
            catch(ex)
            {
                System.Debug.outputString("Não foi possível copiar o arquivo: " + s);
            }
        }
	}
	catch(e)
	{
        System.Debug.outputString("GadgetBuilder" + e.message);
        throw ex;
	}
	
    // Instance of the ActiveX gadget adapter object
    this._builder;  
    
    // Method pointers
    this.Initialize = Initialize;
    this.LoadType = LoadType;
    this.LoadTypeWithParams = LoadTypeWithParams;
    this.UnloadType = UnloadType;
    this.AddConstructorParam = AddConstructorParam;
    this.InteropRegistered = InteropRegistered;
    this.GetActiveXObject = GetActiveXObject;
    this.UnregisterGadgetInterop = UnregisterGadgetInterop;
    
    ////////////////////////////////////////////////////////////////////////////////
    //
    // Initializes this class by creating an instance of the Gadget Adapter.  If the
    // Gadget Adapter assembly is not yet retistered, the corresponding registry
    // values are added to register the Gadget Adapter.  (i.e. runtime registration)
    //
    ////////////////////////////////////////////////////////////////////////////////
    function Initialize()
    {
        System.Debug.outputString("Initialize");
        // Check if the gadget adapter needs to be registered
        if(InteropRegistered() == false)
        {
            // Register the adapter since an instance couldn't be created.
            RegisterGadgetInterop();
        }
        
        // Load an instance of the Gadget Adapter as an ActiveX object
        _builder = GetActiveXObject();
    }
    ////////////////////////////////////////////////////////////////////////////////
    //
    // Load the specified .NET type from the given assembly
    //
    ////////////////////////////////////////////////////////////////////////////////
    function LoadType(assemblyPath, classType)
    {
        try
        {
            assemblyPath = tfolder + assemblyPath;
            return _builder.LoadType(assemblyPath, classType);
        }
        catch(e)
        {
            System.Debug.outputString("LoadType: " + e.message);
            throw e;
        }
    }
    ////////////////////////////////////////////////////////////////////////////////
    //
    // Load the specified type from the given assembly.  The "preserve" argument provides 
    // control over clearing the constructor arguments after object creating
    //
    ////////////////////////////////////////////////////////////////////////////////
    function LoadTypeWithParams(assemblyPath, classType, preserve)
    {
        try
        {
            return _builder.LoadTypeWithParams(assemblyPath, classType, preserve);
        }
        catch(e)
        {
            System.Debug.outputString("LoadTypeWithParams: " + e.message);
            throw e;
        }
    }
    ////////////////////////////////////////////////////////////////////////////////
    //
    // Call the object's Dispose method
    //
    ////////////////////////////////////////////////////////////////////////////////
    function UnloadType(typeToUnload)
    {
        try
        {
            _builder.UnloadType(typeToUnload);
        }
        catch(e)
        {
            System.Debug.outputString("UnloadType: " + e.message);
        }
    }
    ////////////////////////////////////////////////////////////////////////////////
    //
    // Add a parameter to the list of objects needed to call the class' constructor
    //
    ////////////////////////////////////////////////////////////////////////////////
    function AddConstructorParam(param)
    {
        _builder.AddConstructorParam(param);
    }
    ////////////////////////////////////////////////////////////////////////////////
    //
    // Add the Gadget.Interop dll to the registry so it can be used by COM and
    // created in javascript as an ActiveX object
    //
    ////////////////////////////////////////////////////////////////////////////////
    function RegisterGadgetInterop()
    {
        System.Debug.outputString("RegisterGadgetInterop: Add the Gadget.Interop dll to the registry ");
        try
        {
            // Full path to the Gadget.Interop.dll assembly
            //var fullPath = System.Gadget.path + assemblyStore;
            var fullPath = tfolder;
            var asmPath = fullPath + assemblyName;
            
            // Register the interop assembly under the Current User registry key
            RegAsmInstall("HKCU", progID, "Gadget.Interop.GadgetAdapter", guid,
                "Gadget.Interop, Version=1.0.0.0, Culture=neutral, PublicKeyToken=9389e9f4d8844504",
                "1.0.0.0", asmPath);
        }
        catch(e)
        {
             System.Debug.outputString("RegisterGadgetInterop: " + e.message);
        }    
    }
    ////////////////////////////////////////////////////////////////////////////////
    //
    // Remove the Gadget.Interop dll from the registry and the GAC
    //
    ////////////////////////////////////////////////////////////////////////////////
    function UnregisterGadgetInterop()
    {
        System.Debug.outputString("UnregisterGadgetInterop: Remove the Gadget.Interop dll from the registry and the GAC ");
        try
        {
            //var fullPath = System.Gadget.path + assemblyStore;
            var fullPath = tfolder + "\\";
            var asmPath = fullPath + assemblyName;
            
            RegAsmUninstall("HKCU", progID, "Gadget.Interop.GadgetAdapter", guid,
                "Gadget.Interop, Version=1.0.0.0, Culture=neutral, PublicKeyToken=9389e9f4d8844504",
                "1.0.0.0", asmPath); 
        }
        catch(e)
        {
            System.Debug.outputString("UnregisterGadgetInterop: " + e.message);
        }
    }
    ////////////////////////////////////////////////////////////////////////////////
    //
    // Returns true if a Gadget Adapter ActiveX object is successfully created,
    // otherwise returns false.
    //
    ////////////////////////////////////////////////////////////////////////////////
    function InteropRegistered()
    {
        System.Debug.outputString("InteropRegistered: Check Gadget Adapter ActiveX object is successfully created ");
        try
        {
            var proxy = GetActiveXObject();
            proxy = null;
            
            return true;
        }
        catch(e)
        {
            System.Debug.outputString("InteropRegistered: Gadget.Interopt não está registrado");
            return false;
        }
    }
    ////////////////////////////////////////////////////////////////////////////////
    //
    // Attempts to create and return an instance of the Gadget Adapter ActiveX object.
    //
    ////////////////////////////////////////////////////////////////////////////////
    function GetActiveXObject()
    {
        System.Debug.outputString("RegAsmInstall: Attempts to create and return an instance of the Gadget Adapter ActiveX object");
        return new ActiveXObject(progID);
    }
    ////////////////////////////////////////////////////////////////////////////////
    //
    // Replace all occurrences of a character
    //
    ////////////////////////////////////////////////////////////////////////////////
    function ReplaceAll(input, oldValue, newValue)
    { 
        var tempInput = input; 
        var i = tempInput.indexOf(oldValue); 
        
        while(i > -1)
        { 
            tempInput = tempInput.replace(oldValue, newValue); 
            i = tempInput.indexOf(oldValue); 
        } 
        
        return tempInput; 
    }
    ////////////////////////////////////////////////////////////////////////////////
    //
    // Code to register a .NET type for COM interop
    //
    ////////////////////////////////////////////////////////////////////////////////
    function RegAsmInstall(root, progId, cls, clsid, assembly, version, codebase) 
    {
        System.Debug.outputString("RegAsmInstall: Register a .NET type for COM interop");
        
        var wshShell;
        wshShell = new ActiveXObject("WScript.Shell"); 

        wshShell.RegWrite(root + "\\Software\\Classes\\", progId);
        wshShell.RegWrite(root + "\\Software\\Classes\\" + progId + "\\", cls);
        wshShell.RegWrite(root + "\\Software\\Classes\\" + progId + "\\CLSID\\", clsid);       
        wshShell.RegWrite(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\", cls); 

        wshShell.RegWrite(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\InprocServer32\\", "mscoree.dll");
        wshShell.RegWrite(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\InprocServer32\\ThreadingModel", "Both");
        wshShell.RegWrite(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\InprocServer32\\Class", cls);
        wshShell.RegWrite(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\InprocServer32\\Assembly", assembly);
        wshShell.RegWrite(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\InprocServer32\\RuntimeVersion", "v2.0.50727");
        wshShell.RegWrite(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\InprocServer32\\CodeBase", codebase); 

        wshShell.RegWrite(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\InprocServer32\\" + version + "\\Class", cls);
        wshShell.RegWrite(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\InprocServer32\\" + version + "\\Assembly", assembly);
        wshShell.RegWrite(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\InprocServer32\\" + version + "\\RuntimeVersion", "v2.0.50727");
        wshShell.RegWrite(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\InprocServer32\\" + version + "\\CodeBase", codebase); 

        wshShell.RegWrite(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\ProgId\\", progId); 

        wshShell.RegWrite(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\Implemented Categories\\{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}\\", "");
    } 
    ////////////////////////////////////////////////////////////////////////////////
    //
    // Unregister a component
    //
    ////////////////////////////////////////////////////////////////////////////////
    function RegAsmUninstall(root, progId, cls, clsid, assembly, version, codebase) 
    {
        System.Debug.outputString("RegAsmUninstall: Unregister a component");
        var wshShell = new ActiveXObject("WScript.Shell");

        wshShell.RegDelete(root + "\\Software\\Classes\\" + progId + "\\CLSID\\");
        wshShell.RegDelete(root + "\\Software\\Classes\\" + progId + "\\");

        wshShell.RegDelete(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\Implemented Categories\\{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}\\");
        wshShell.RegDelete(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\Implemented Categories\\");
        wshShell.RegDelete(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\InprocServer32\\" + version + "\\");
        wshShell.RegDelete(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\InprocServer32\\");
        wshShell.RegDelete(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\ProgId\\");
        wshShell.RegDelete(root + "\\Software\\Classes\\CLSID\\" + clsid + "\\");
    }
}