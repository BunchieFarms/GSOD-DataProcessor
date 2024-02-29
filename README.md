# GSOD (Global Summary of the Day) Data Processor
This is a project I started because I wanted to know what the weather was like in an arbitrary location a day or two ago. I enjoy mountain biking, and some trails are a few hours away, so unless I am paying attention and planning my trip well, I won't know if the trail is closed due to recent rainfall. So why not look up past weather in that area? This is sometimes a bit difficult.

# The problem with past weather data
When I first had this idea, I tried using some of the many weather APIs to get past data. The problem is, just about every one of them use past *forecast* data instead of actual observed data. This is because, this observed weather comes from stations all around the world that submit their data to different datasets. Usually, they are a day or two behind on submitting their data, so this is why you can't usually find observed past weather data for the day before.

# My solution...
...Is not all that much better. I can't get data for a day that hasn't been submitted yet. So, I do the best I can by pulling from the NOAA GSOD dataset to get the most recent daily data I can. I use this data for another app, [Weathered](http://weathered.bryceohmer.com), but I just fill in the gaps with stored recent forecast data from Pirate Weather. Hopefully I'll find a better dataset one day, or learn how to use the wacky wgrib2 files that the NOAA stores forecast data in using C# because I am a brainlet.

# Run it yourself
You will need MongoDB installed with a database containing the collections named in Shared/Constants.cs. I named my database "Weathered", but you can name all of the items whatever you want. You'll also need the GSOD dataset URI, I just use the https server but I'm sure there is another way to do it.