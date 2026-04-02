from openai.lib.azure import AzureOpenAI, AsyncAzureOpenAI, AzureADTokenProvider, AsyncAzureADTokenProvider
scopes = "https://cognitiveservices.azure.com/.default"
# May change in the future
endpoint = "https://my-resource.openai.azure.com"
deployment_name = "deployment-name"  # e.g. gpt-35-instant
def sync_main() -> None:
    from azure.identity import DefaultAzureCredential, get_bearer_token_provider
    token_provider: AzureADTokenProvider = get_bearer_token_provider(DefaultAzureCredential(), scopes)
        azure_endpoint=endpoint,
        azure_ad_token_provider=token_provider,
        model=deployment_name,
async def async_main() -> None:
    from azure.identity.aio import DefaultAzureCredential, get_bearer_token_provider
    token_provider: AsyncAzureADTokenProvider = get_bearer_token_provider(DefaultAzureCredential(), scopes)
    client = AsyncAzureOpenAI(
    completion = await client.chat.completions.create(
sync_main()
asyncio.run(async_main())
