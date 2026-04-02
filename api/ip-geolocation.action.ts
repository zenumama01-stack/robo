 * Action that retrieves geolocation information for an IP address using free public APIs.
 * Provides location, timezone, ISP, and other relevant data about the IP address.
 * // Get location for specific IP
 *   ActionName: 'IP Geolocation',
 *     Name: 'IPAddress',
 *     Value: '8.8.8.8'
 * // Get location for current user's IP
 *   Params: []  // No IP provided will use caller's IP
@RegisterClass(BaseAction, "__IPGeolocation")
export class IPGeolocationAction extends BaseAction {
     * Executes the IP geolocation lookup
     *   - IPAddress: The IP address to lookup (optional - uses caller's IP if not provided)
     * @returns Geolocation data including country, city, coordinates, timezone, etc.
            const ipParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'ipaddress');
            let ipAddress = ipParam?.Value || '';
            // Validate IP address format if provided
            if (ipAddress && !this.isValidIP(ipAddress)) {
                    Message: "Invalid IP address format",
                    ResultCode: "INVALID_IP"
            // Use multiple free APIs for redundancy and more complete data
            const geoData = await this.fetchGeolocationData(ipAddress);
            if (!geoData) {
                    Message: "Failed to retrieve geolocation data",
                    ResultCode: "LOOKUP_FAILED"
            // Calculate additional useful information
            const currentTime = geoData.timezone ? 
                new Date().toLocaleString('en-US', { timeZone: geoData.timezone }) : null;
                ip: geoData.ip,
                    country: geoData.country,
                    countryCode: geoData.countryCode,
                    region: geoData.region,
                    regionCode: geoData.regionCode,
                    city: geoData.city,
                    postalCode: geoData.postalCode,
                        latitude: geoData.latitude,
                        longitude: geoData.longitude
                    accuracy: geoData.accuracy || 'city'
                timezone: {
                    id: geoData.timezone,
                    offset: geoData.timezoneOffset,
                    currentTime: currentTime
                network: {
                    isp: geoData.isp,
                    organization: geoData.org,
                    asn: geoData.asn,
                    type: this.detectIPType(geoData)
                    isVPN: geoData.vpn || false,
                    isProxy: geoData.proxy || false,
                    isTor: geoData.tor || false,
                    isHosting: geoData.hosting || false
                continent: geoData.continent,
                languages: geoData.languages,
                currency: geoData.currency,
                callingCode: geoData.callingCode
                Message: `Failed to get IP geolocation: ${error instanceof Error ? error.message : String(error)}`,
    private async fetchGeolocationData(ipAddress: string): Promise<any> {
        // Try multiple free APIs in order of preference
        const apis = [
                name: 'ipapi.co',
                url: ipAddress ? `https://ipapi.co/${ipAddress}/json/` : 'https://ipapi.co/json/',
                transform: (data: any) => ({
                    ip: data.ip,
                    country: data.country_name,
                    countryCode: data.country_code,
                    region: data.region,
                    regionCode: data.region_code,
                    city: data.city,
                    postalCode: data.postal,
                    latitude: data.latitude,
                    longitude: data.longitude,
                    timezone: data.timezone,
                    timezoneOffset: data.utc_offset,
                    isp: data.org,
                    org: data.org,
                    asn: data.asn,
                    continent: data.continent_code,
                    languages: data.languages?.split(',') || [],
                    currency: data.currency,
                    callingCode: data.country_calling_code
                name: 'ip-api.com',
                url: `http://ip-api.com/json/${ipAddress || ''}?fields=status,message,continent,continentCode,country,countryCode,region,regionName,city,district,zip,lat,lon,timezone,offset,currency,isp,org,as,asname,reverse,mobile,proxy,hosting,query`,
                transform: (data: any) => {
                    if (data.status !== 'success') {
                        throw new Error(data.message || 'API request failed');
                        ip: data.query,
                        country: data.country,
                        countryCode: data.countryCode,
                        region: data.regionName,
                        regionCode: data.region,
                        postalCode: data.zip,
                        latitude: data.lat,
                        longitude: data.lon,
                        timezoneOffset: data.offset,
                        isp: data.isp,
                        asn: data.as,
                        continent: data.continentCode,
                        proxy: data.proxy,
                        hosting: data.hosting,
                        mobile: data.mobile
        // Try each API until one succeeds
        for (const api of apis) {
                const response = await axios.get(api.url, {
                    timeout: 5000,
                        'User-Agent': 'MemberJunction-IPGeolocation-Action/1.0'
                    return api.transform(response.data);
                // Continue to next API
        // If we have an IP, try a basic API that just gives country
        if (ipAddress) {
                const response = await axios.get(`https://api.country.is/${ipAddress}`);
                        ip: ipAddress,
                        country: response.data.country,
                        countryCode: response.data.country
                // Fall through
    private isValidIP(ip: string): boolean {
        // IPv4 pattern
        const ipv4Pattern = /^(\d{1,3}\.){3}\d{1,3}$/;
        // IPv6 pattern (simplified)
        const ipv6Pattern = /^([\da-f]{1,4}:){7}[\da-f]{1,4}$/i;
        const ipv6CompressedPattern = /^([\da-f]{1,4}:){1,7}:$/i;
        if (ipv4Pattern.test(ip)) {
            // Validate IPv4 octets
            const octets = ip.split('.');
            return octets.every(octet => {
                const num = parseInt(octet, 10);
                return num >= 0 && num <= 255;
        return ipv6Pattern.test(ip) || ipv6CompressedPattern.test(ip);
    private detectIPType(geoData: any): string {
        if (geoData.mobile) return 'Mobile';
        if (geoData.hosting) return 'Hosting/Datacenter';
        if (geoData.proxy) return 'Proxy';
        if (geoData.vpn) return 'VPN';
        if (geoData.tor) return 'Tor';
        if (geoData.org?.toLowerCase().includes('corporate')) return 'Corporate';
        return 'Residential';
