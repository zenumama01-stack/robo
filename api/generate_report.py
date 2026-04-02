"""Retrieves a saved report, or a report for the specified ad client.
To get ad clients, run get_all_ad_clients.py.
Tags: reports.generate
__author__ = "sgomes@google.com (Sérgio Gomes)"
from oauth2client import client
# Declare command-line flags.
argparser = argparse.ArgumentParser(add_help=False)
argparser.add_argument(
    "--ad_client_id", help="The ID of the ad client for which to generate a report"
argparser.add_argument("--report_id", help="The ID of the saved report to generate")
    # Authenticate and construct service.
    service, flags = sample_tools.init(
        argv,
        "adexchangeseller",
        "v1.1",
        __doc__,
        __file__,
        parents=[argparser],
        scope="https://www.googleapis.com/auth/adexchange.seller.readonly",
    # Process flags and read their values.
    ad_client_id = flags.ad_client_id
    saved_report_id = flags.report_id
        # Retrieve report.
        if saved_report_id:
            result = (
                service.reports()
                .saved()
                .generate(savedReportId=saved_report_id)
                .execute()
        elif ad_client_id:
                .generate(
                    startDate="2011-01-01",
                    endDate="2011-08-31",
                    filter=["AD_CLIENT_ID==" + ad_client_id],
                    metric=[
                        "PAGE_VIEWS",
                        "AD_REQUESTS",
                        "AD_REQUESTS_COVERAGE",
                        "CLICKS",
                        "AD_REQUESTS_CTR",
                        "COST_PER_CLICK",
                        "AD_REQUESTS_RPM",
                        "EARNINGS",
                    dimension=["DATE"],
                    sort=["+DATE"],
            argparser.print_help()
        # Display headers.
        for header in result["headers"]:
            print("%25s" % header["name"], end=" ")
        # Display results.
        for row in result["rows"]:
            for column in row:
                print("%25s" % column, end=" ")
    except client.AccessTokenRefreshError:
            "The credentials have been revoked or expired, please re-run the "
            "application to re-authorize"
    main(sys.argv)
