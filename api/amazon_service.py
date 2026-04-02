from email.mime.text import MIMEText
    """Text to speech"""
def send_email(subject, body, recipient_email):
    Send email notification.
    Configure your email credentials in config.
        # Note: Replace with your actual email credentials
        sender_email = "your_email@gmail.com"
        sender_password = "your_app_password"
        server.login(sender_email, sender_password)
        msg = MIMEText(body)
        msg['Subject'] = subject
        msg['From'] = sender_email
        msg['To'] = recipient_email
        server.sendmail(sender_email, recipient_email, msg.as_string())
        print(f"Email sent successfully to {recipient_email}")
        print(f"Error sending email: {e}")
        return False
def check_amazon_price(product_url, target_price=None, recipient_email=None):
    Check Amazon product price.
        product_url: Amazon product URL
        target_price: Alert if price drops below this
        recipient_email: Email to send alert to
            "User-Agent": 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
        response = requests.get(product_url, headers=headers, timeout=10)
        response.raise_for_status()
        soup = BeautifulSoup(response.content, 'html.parser')
        title_elem = soup.find(id='productTitle')
        title = title_elem.get_text().strip() if title_elem else "Unknown Product"
        price_elem = soup.find(id='priceblock_dealprice')
        if not price_elem:
            price_elem = soup.find(id='priceblock_ourprice')
        price_text = price_elem.get_text().strip() if price_elem else None
        if price_text:
            # Extract numeric price
            price_str = ''.join(c for c in price_text if c.isdigit() or c == '.')
            price = float(price_str) if price_str else None
            print(f"Product: {title}")
            print(f"Current Price: {price_text}")
            speak(f"Product price is {price_text}")
            # Send alert if price dropped
            if target_price and price and price <= target_price:
                message = f"Alert! Price of {title} dropped to {price_text}"
                print(message)
                speak(message)
                if recipient_email:
                    send_email(
                        subject=f"Price Drop Alert: {title}",
                        body=f"{message}\n\nURL: {product_url}",
                        recipient_email=recipient_email
            return {
                'title': title,
                'price': price,
                'price_text': price_text,
                'url': product_url
            print("Could not find price information")
        print(f"Error checking Amazon price: {e}")
        speak(f"Error checking price: {str(e)}")
def check_prices():
    Main function to check prices of configured products.
    Configure your products and email settings below.
    # Example products to monitor
    products = [
        {
            'url': 'https://www.amazon.com/example-product',
            'target_price': 50.0,  # Alert if price drops below this
            'name': 'Example Product'
    recipient_email = "your_email@gmail.com"  # Configure your email
    speak("Checking prices for all configured products")
    results = []
    for product in products:
        print(f"\nChecking {product['name']}...")
        result = check_amazon_price(
            product_url=product['url'],
            target_price=product.get('target_price'),
            results.append(result)
    speak(f"Price check completed. Found {len(results)} products")
    return results
    # Test
    check_prices()
