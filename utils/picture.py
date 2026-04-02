prompt = "An astronaut lounging in a tropical resort in space, pixel art"
model = "dall-e-3"
    # Generate an image based on the prompt
    response = openai.images.generate(prompt=prompt, model=model)
    # Prints response containing a URL link to image
    print(response)
