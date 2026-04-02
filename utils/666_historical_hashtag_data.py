posts = instaloader.Hashtag.from_name(L.context, "urbanphotography").get_posts_resumable()
SINCE = datetime(2020, 5, 10)  # further from today, inclusive
UNTIL = datetime(2020, 5, 11)  # closer to today, not inclusive
k = 0  # initiate k
#k_list = []  # uncomment this to tune k
    postdate = post.date
    if postdate > UNTIL:
    elif postdate <= SINCE:
        k += 1
        if k == 50:
        L.download_post(post, "#urbanphotography")
        # if you want to tune k, uncomment below to get your k max
        #k_list.append(k)
        k = 0  # set k to 0
#max(k_list)
