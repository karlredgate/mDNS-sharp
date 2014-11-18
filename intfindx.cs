
using System;                           // for Serializable
using System.Runtime.InteropServices;   // for StructLayout

using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Collections;               // for ArrayList
using System.Collections.Generic; // for List<>

public class InterfaceIndexer {
    public InterfaceIndexer() { }

    public static int 
    BestInterfaceIndex() {
	NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
	foreach ( NetworkInterface nic in nics ) {
            System.Console.WriteLine( "NIC " + nic.ToString() );
	    if ( nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet ) {
		System.Console.WriteLine( "Type is " + nic.NetworkInterfaceType );
	        continue;
	    }
	    System.Console.WriteLine( "Type is " + nic.NetworkInterfaceType );
	    if ( nic.Supports(NetworkInterfaceComponent.IPv4) == false ) {
		System.Console.WriteLine( "Intf " + nic.ToString() + " does not support IPv4" );
	        continue;
	    }
	    System.Console.WriteLine( "Intf " + nic.ToString() + " supports IPv4" );
	    IPInterfaceProperties properties = nic.GetIPProperties();
	    IPv4InterfaceProperties v4_props = properties.GetIPv4Properties();
	    const int mask = (1<<16)-1;
	    int index = v4_props.Index & mask;
	    return index;
	}
	throw new SystemException("could not find an interface index");
    }

    public static void Main() {
        System.Console.WriteLine( "Check interface indexes" );
        System.Console.WriteLine( "Best index is " + BestInterfaceIndex() );
    }
}
