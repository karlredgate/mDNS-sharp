
/**
 * See:
 * http://msdn.microsoft.com/en-us/library/system.net.networkinformation.networkinterface.description.aspx
 * http://msdn.microsoft.com/en-us/library/system.net.networkinformation.ipaddressinformation(v=vs.100).aspx
 */

using System;
using System.Net;
using System.Net.NetworkInformation;

class InterfaceCLI {

    public static int BestInterfaceIndex() {
        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
        foreach ( NetworkInterface nic in nics ) {
	    if ( nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet ) continue;
            if ( nic.Supports(NetworkInterfaceComponent.IPv4) == false ) continue;
            IPInterfaceProperties properties = nic.GetIPProperties();
            IPv4InterfaceProperties v4_props = properties.GetIPv4Properties();
            return v4_props.Index;
        }
	throw new SystemException("could not find an interface index");
    }

    public static void IPv4Report( IPInterfaceProperties properties ) {
        System.Console.Write( "   IPv4: " );
        IPv4InterfaceProperties v4_props = properties.GetIPv4Properties();
        System.Console.Write( " Index:{0}", v4_props.Index );
        System.Console.Write( " MTU:{0}", v4_props.Mtu );
        System.Console.Write( " Fwd:{0}", v4_props.IsForwardingEnabled );
        System.Console.WriteLine();
    }

    public static void IPv6Report( IPInterfaceProperties properties ) {
        System.Console.Write( "   IPv6: " );
        // IPv6InterfaceProperties v6_props = properties.GetIPv6Properties();
        // System.Console.Write( " Index:{0}", v6_props.Index );
        // System.Console.Write( " MTU:{0}", v6_props.Mtu );
        // System.Console.Write( " Fwd:{0}", v6_props.IsForwardingEnabled );
        System.Console.WriteLine();
    }

    public static void InterfaceReport( NetworkInterface nic ) {
        IPInterfaceProperties properties = nic.GetIPProperties();
        int intfidx = 0; // properties.Index
        System.Console.WriteLine( "{0}:  {1}  ({2})", intfidx, nic.Name, nic.Description );
        System.Console.Write( "   " );
        System.Console.WriteLine( "Type:{0}  Status:{1}", nic.NetworkInterfaceType, nic.OperationalStatus );
        // UnicastIPAddressInformation address
        foreach ( var info in properties.UnicastAddresses ) {
            System.Console.WriteLine( info.Address );
        }

        if ( nic.Supports(NetworkInterfaceComponent.IPv4) ) {
            IPv4Report( properties );
        }

        if ( nic.Supports(NetworkInterfaceComponent.IPv6) ) {
            IPv6Report( properties );
        }
    }

    public static void Main( string[] args ) {

        // IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        // System.Console.WriteLine( "Hostname: {0}", properties.HostName );
        // System.Console.WriteLine( "Domain:   {0}", properties.DomainName );

        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

        foreach ( NetworkInterface nic in nics ) {
            InterfaceReport( nic );
        }
	System.Console.WriteLine( "Interface index: " + BestInterfaceIndex() );
    }
}
